import re
from io import BytesIO
import pandas as pd
import numpy as np

INCLUDE_TYPES = [
    "breakdown",
    "corrective",
    "preventive",
    "preventative",
    "pm",
    "inspection",
    "campaign",
]


def _best_col(df, patterns):
    cols = list(df.columns)
    lc = {c.lower(): c for c in cols}
    for p in patterns:
        for c_l, c in lc.items():
            if p in c_l:
                return c
    return None


def load_workbook(path_or_buffer):
    return pd.read_excel(path_or_buffer, engine="openpyxl")


def normalize_df(df: pd.DataFrame) -> pd.DataFrame:
    # Map likely column names to standard names
    col_map = {}
    mappings = {
        "work_order": ["work_order", "wo", "workorder", "work order"],
        "equipment": ["equipment", "asset", "equipment id", "asset id"],
        "type": ["type", "work_type", "work type", "wo_type"],
        "assigned_to": ["assigned_to", "assigned", "tech", "assigned to", "assignee"],
        "date_reported": ["date_reported", "reported", "opened", "date reported", "date_reported"],
        "date_completed": ["date_completed", "completed", "closed", "date completed", "date_completed"],
    }

    for std, pats in mappings.items():
        col = _best_col(df, pats)
        if col:
            col_map[col] = std

    df = df.rename(columns=col_map)

    # Ensure required columns exist; create empty if missing to avoid crashes
    for c in ["work_order", "equipment", "type", "assigned_to", "date_reported", "date_completed"]:
        if c not in df.columns:
            df[c] = pd.NA

    # Parse dates
    df["date_reported"] = pd.to_datetime(df["date_reported"], errors="coerce")
    df["date_completed"] = pd.to_datetime(df["date_completed"], errors="coerce")

    # Normalize strings
    df["type"] = df["type"].astype(str).fillna("").str.lower()
    df["equipment"] = df["equipment"].astype(str).fillna("")
    df["assigned_to"] = df["assigned_to"].astype(str).fillna("")
    df["work_order"] = df["work_order"].astype(str).fillna("")

    # Extract asset_number: last 5-6 digits if present
    def extract_asset(s: str):
        m = re.search(r"(\d{5,6})$", s)
        if m:
            return m.group(1)
        # fallback: any trailing digits
        m2 = re.search(r"(\d{4,6})", s)
        return m2.group(1) if m2 else s

    df["asset_number"] = df["equipment"].apply(extract_asset)

    # Filter to include iBot-related maintenance rows: either equipment contains 'ibot' or type matches include list
    mask1 = df["equipment"].str.lower().str.contains("ibot", na=False)
    mask2 = df["type"].apply(lambda v: any(k in v for k in INCLUDE_TYPES))
    df = df[mask1 | mask2].copy()

    # Drop rows without date_completed - week/day assignment relies on it
    df = df.dropna(subset=["date_completed"]).copy()

    # Day and ISO week (use ISO week of date_completed)
    isocal = df["date_completed"].dt.isocalendar()
    df["iso_week"] = isocal.week.astype(int)
    # Cap weeks into 1..52 as user requested Weeks 1-52 (put 53 into 52)
    df["iso_week"] = df["iso_week"].clip(1, 52)
    df["day"] = df["date_completed"].dt.date

    return df


def compute_metrics(df: pd.DataFrame) -> pd.DataFrame:
    df = normalize_df(df)

    # STEP A — Remove same-tech duplicates (same work_order, asset, assigned_to, day)
    df_a = df.drop_duplicates(subset=["work_order", "asset_number", "assigned_to", "day"]).copy()

    # For daily completions: each unique (asset_number, assigned_to) pair counts once per day
    pairs = df_a[["day", "asset_number", "assigned_to"]].drop_duplicates()
    daily_completions = (
        pairs.groupby("day").size().rename("daily_completions").astype(int)
    )

    # Daily returns: asset is a return if it appears more than once in same day AND two or more different techs worked on it
    grouped_asset_day = df_a.groupby(["day", "asset_number"]).agg(
        occurrences=("work_order", "count"),
        techs=("assigned_to", lambda s: s.nunique()),
    )
    grouped_asset_day["is_return"] = (grouped_asset_day["occurrences"] > 1) & (grouped_asset_day["techs"] >= 2)
    daily_returns = grouped_asset_day.reset_index().groupby("day")["is_return"].sum().rename("returned_bots").astype(int)

    # Combine daily stats
    daily = pd.concat([daily_completions, daily_returns], axis=1).fillna(0)
    daily.index = pd.to_datetime(daily.index)

    daily["daily_completions"] = daily["daily_completions"].astype(int)
    daily["returned_bots"] = daily["returned_bots"].astype(int)

    # Daily Return % (skip days with 0 comps)
    daily_active = daily[daily["daily_completions"] > 0].copy()
    daily_active["daily_return_pct"] = (daily_active["returned_bots"] / daily_active["daily_completions"]) * 100

    # Map each active day to iso_week. We need iso_week for each day — get from original df
    # Use the 'day' (date object) as mapping keys so lookups from Timestamp.date() succeed
    day_to_week = df.drop_duplicates(subset=["day"]).set_index("day")["iso_week"].to_dict()
    daily_active["iso_week"] = daily_active.index.map(lambda d: day_to_week.get(d.date(), np.nan)).astype(pd.Int64Dtype())

    # Weekly Return % = average of daily_return_pct across active days only
    weekly_return = (
        daily_active.groupby("iso_week")["daily_return_pct"].mean().rename("weekly_return_pct")
    )

    # WEEKLY other metrics
    # Work orders assigned to week by date_completed
    # Compute TTR hours per work_order
    df_orders = df.copy()
    df_orders["ttr_hours"] = (df_orders["date_completed"] - df_orders["date_reported"]).dt.total_seconds() / 3600.0

    # Unique work orders per week
    wo_volume = df_orders.groupby("iso_week")["work_order"].nunique().rename("wo_volume")

    # Weekly MTTR (median of TTR hours for unique work_orders) — compute per work_order first
    wo_ttr = df_orders.groupby(["iso_week", "work_order"])['ttr_hours'].first().reset_index()
    weekly_mttr = wo_ttr.groupby("iso_week")["ttr_hours"].median().rename("mttr_hrs")

    # % closed <=24, <=48
    def pct_leq(hours):
        mask = wo_ttr['ttr_hours'].notna()
        total = wo_ttr[mask].groupby('iso_week')['work_order'].nunique()
        leq = wo_ttr[wo_ttr['ttr_hours'] <= hours].groupby('iso_week')['work_order'].nunique()
        return (leq / total * 100).rename(f"pct_leq_{int(hours)}h")

    pct24 = pct_leq(24)
    pct48 = pct_leq(48)

    # Assemble weekly table for weeks 1..52
    weeks = pd.Series(range(1, 53), name='iso_week')
    result = pd.DataFrame(weeks)
    result = result.set_index('iso_week')

    result = result.join(weekly_return).join(weekly_mttr).join(wo_volume).join(pct24).join(pct48)

    # Fill rules: If week has no qualifying iBot work, include it as WO Volume=0 Return%=0.00% MTTR and closure % = blank
    result['wo_volume'] = result['wo_volume'].fillna(0).astype(int)
    result['weekly_return_pct'] = result['weekly_return_pct'].fillna(0.0)

    # MTTR and pct columns: if no WOs, set to NaN to allow consumer to choose blank/N/A
    result['mttr_hrs'] = result['mttr_hrs'].where(result['wo_volume'] > 0)
    result['pct_leq_24h'] = result['pct_leq_24h'].where(result['wo_volume'] > 0)
    result['pct_leq_48h'] = result['pct_leq_48h'].where(result['wo_volume'] > 0)

    # Formatting columns
    result = result.rename(columns={
        'weekly_return_pct': 'Weekly Return %',
        'mttr_hrs': 'MTTR (hrs)',
        'wo_volume': 'WO Volume',
        'pct_leq_24h': '% ≤24 hrs',
        'pct_leq_48h': '% ≤48 hrs',
    })

    # Ensure Week column present
    result = result.reset_index().rename(columns={'iso_week': 'Week'})

    # Round percentages to 2 decimals
    result['Weekly Return %'] = result['Weekly Return %'].round(2)
    result['% ≤24 hrs'] = result['% ≤24 hrs'].round(2)
    result['% ≤48 hrs'] = result['% ≤48 hrs'].round(2)

    return result


def process_file(path_or_buffer) -> pd.DataFrame:
    df = load_workbook(path_or_buffer)
    return compute_metrics(df)
