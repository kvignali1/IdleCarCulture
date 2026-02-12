import os
from pathlib import Path
from datetime import datetime

import streamlit as st
import pandas as pd
from percent_return.processor import process_file, load_workbook, normalize_df
from io import BytesIO
import shutil


st.set_page_config(page_title="% of Return — iBot Metrics", layout="wide")

# Data directory for saved uploads and results
DATA_DIR = Path("data")
DATA_DIR.mkdir(parents=True, exist_ok=True)
LOG_PATH = DATA_DIR / "uploads_log.csv"
MASTER_PATH = DATA_DIR / "master_weeks.csv"
HISTORY_DIR = DATA_DIR / "history"
HISTORY_DIR.mkdir(parents=True, exist_ok=True)
HISTORY_INDEX = DATA_DIR / "history_index.csv"


def load_master():
    if MASTER_PATH.exists():
        df = pd.read_csv(MASTER_PATH)
        if 'Week' in df.columns:
            try:
                df['Week'] = df['Week'].astype(int)
            except Exception:
                df['Week'] = pd.to_numeric(df['Week'], errors='coerce').fillna(0).astype(int)
        return df
    return None


def compute_previous_month_summary(master_df: pd.DataFrame):
    # Define previous month as the last 4 active weeks in master (most recent non-zero WO Volume weeks)
    if master_df is None:
        return None
    # select weeks with WO Volume > 0
    active = master_df[master_df['WO Volume'].fillna(0) > 0].copy()
    if active.empty:
        return None
    active_sorted = active.sort_values('Week')
    last_weeks = active_sorted.tail(4)

    # Weekly Return % average across active weeks
    weekly_return_avg = last_weeks['Weekly Return %'].dropna().mean()

    # MTTR median
    mttr_med = last_weeks['MTTR (hrs)'].dropna().median()

    # WO Volume sum
    wo_sum = int(last_weeks['WO Volume'].sum())

    # Reconstruct counts for <=24 and <=48 from percentages
    def reconstruct_count(row, pct_col):
        try:
            pct = float(row.get(pct_col, 0))
            vol = int(row.get('WO Volume', 0))
            return round(pct / 100.0 * vol)
        except Exception:
            return 0

    last_weeks = last_weeks.copy()
    last_weeks['_leq24_count'] = last_weeks.apply(lambda r: reconstruct_count(r, '% ≤24 hrs'), axis=1)
    last_weeks['_leq48_count'] = last_weeks.apply(lambda r: reconstruct_count(r, '% ≤48 hrs'), axis=1)

    pct24 = (last_weeks['_leq24_count'].sum() / wo_sum * 100) if wo_sum > 0 else None
    pct48 = (last_weeks['_leq48_count'].sum() / wo_sum * 100) if wo_sum > 0 else None

    return {
        'weekly_return_avg': weekly_return_avg,
        'mttr_median': mttr_med,
        'wo_volume_sum': wo_sum,
        '%_leq24': pct24,
        '%_leq48': pct48,
    }

    # Top-level app tabs
    top_tabs = st.tabs(["Home", "Graphs", "iBots", "Data"])
    home_tab, graphs_tab, ibots_tab, data_tab = top_tabs

    with home_tab:
        st.header("Home")
        st.info("Overview and recent highlights")
        m = load_master()
        if m is not None:
            try:
                summary = compute_previous_month_summary(m)
            except Exception:
                summary = None

            if summary:
                c1, c2, c3, c4 = st.columns(4)
                try:
                    c1.metric("Weekly Return % (avg)", f"{summary['weekly_return_avg']:.2f}%")
                except Exception:
                    c1.metric("Weekly Return % (avg)", "N/A")
                try:
                    c2.metric("MTTR (hrs, median)", f"{summary['mttr_median']:.2f}")
                except Exception:
                    c2.metric("MTTR (hrs, median)", "N/A")
                try:
                    c3.metric("WO Volume (last 4w)", f"{int(summary['wo_volume_sum'])}")
                except Exception:
                    c3.metric("WO Volume (last 4w)", "N/A")
                try:
                    pct24 = summary.get('%_leq24')
                    c4.metric("% ≤24 hrs", f"{pct24:.2f}%" if pct24 is not None else "N/A")
                except Exception:
                    c4.metric("% ≤24 hrs", "N/A")
        else:
            st.info("No master yet. Upload processed results in the Data tab to create a master.")

    with graphs_tab:
        st.header("Graphs")
        st.info("Graphing UI will be added here. No graphs yet.")

    with ibots_tab:
        st.header("iBots with the worst % of return")
        st.info("This tab will list iBots with highest same-day return rates. Placeholder for now.")

    with data_tab:
        st.header("Data — Uploads, history, and master management")

        st.markdown("Upload an Excel file exported from HxGN EAM / APM. The app follows the exact logic for same-day returns and weekly rollup.")

    # Always show master data (read-only by default)
    st.subheader("Master Weeks (read-only)")
    master_df = load_master()
    if master_df is not None:
        st.dataframe(master_df.style.format({
            'Weekly Return %': '{:.2f}%',
            '% ≤24 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
            '% ≤48 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
        }))
    else:
        st.info('No master history exists yet.')

    # Admin-edit controls
    st.markdown('')
    if master_df is not None:
        with st.expander('Admin — edit master (requires password)'):
            if 'is_admin' not in st.session_state:
                st.session_state['is_admin'] = False
            pw = st.text_input('Admin password', type='password', key='admin_pw')
            if st.button('Authenticate', key='auth_btn'):
                if pw == 'SBD1':
                    st.session_state['is_admin'] = True
                    st.success('Admin authenticated — editable mode enabled')
                else:
                    st.session_state['is_admin'] = False
                    st.error('Incorrect admin password')

            if st.session_state.get('is_admin'):
                # Try to show an editable grid; if not available provide CSV replace fallback
                edited = None
                editor_available = True
                try:
                    edited = st.experimental_data_editor(master_df, num_rows='dynamic', key='master_editor')
                except Exception:
                    try:
                        edited = st.data_editor(master_df, key='master_data_editor')
                    except Exception:
                        editor_available = False

                if not editor_available:
                    st.warning('Editable grid not available in this Streamlit version.')
                    st.markdown('Download the master CSV, edit locally, then upload the replacement below.')
                    try:
                        with open(MASTER_PATH, 'rb') as f:
                            st.download_button('Download master CSV', data=f, file_name='master_weeks.csv')
                    except Exception:
                        st.info('Master file not found for download')
                    replacement = st.file_uploader('Upload edited master CSV to replace', type=['csv'], key='replace_master')
                    if replacement is not None and st.button('Replace master with uploaded CSV'):
                        try:
                            new_master = pd.read_csv(replacement)
                            if 'Week' in new_master.columns:
                                new_master['Week'] = new_master['Week'].astype(int)
                                new_master = new_master.sort_values('Week')
                            new_master.to_csv(MASTER_PATH, index=False)
                            st.success('Master replaced from uploaded CSV')
                            # log admin edit
                            ts = datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
                            log_row = {
                                'timestamp_utc': ts,
                                'original_filename': 'admin_replace',
                                'saved_path': '',
                                'results_path': str(MASTER_PATH.as_posix()),
                                'rows_processed': int(getattr(new_master, 'shape', (0,0))[0]),
                                'wo_volume_total': int(new_master['WO Volume'].sum()) if 'WO Volume' in new_master.columns else 0,
                                'overwritten_weeks': 'admin_replace'
                            }
                            if LOG_PATH.exists():
                                log_df = pd.read_csv(LOG_PATH)
                                log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
                            else:
                                log_df = pd.DataFrame([log_row])
                            log_df.to_csv(LOG_PATH, index=False)
                        except Exception as e:
                            st.error(f'Failed to replace master: {e}')
                else:
                    # editable 'edited' DataFrame returned; allow save
                    if edited is not None and st.button('Save master (admin)'):
                        try:
                            to_save = edited.copy()
                            if 'Week' in to_save.columns:
                                to_save['Week'] = to_save['Week'].astype(int)
                                to_save = to_save.sort_values('Week')
                            to_save.to_csv(MASTER_PATH, index=False)
                            st.success('Master updated')
                            # log admin edit
                            ts = datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
                            log_row = {
                                'timestamp_utc': ts,
                                'original_filename': 'admin_edit',
                                'saved_path': '',
                                'results_path': str(MASTER_PATH.as_posix()),
                                'rows_processed': int(getattr(to_save, 'shape', (0,0))[0]),
                                'wo_volume_total': int(to_save['WO Volume'].sum()) if 'WO Volume' in to_save.columns else 0,
                                'overwritten_weeks': 'admin_edit'
                            }
                            if LOG_PATH.exists():
                                log_df = pd.read_csv(LOG_PATH)
                                log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
                            else:
                                log_df = pd.DataFrame([log_row])
                            log_df.to_csv(LOG_PATH, index=False)
                        except Exception as e:
                            st.error(f'Failed to save master: {e}')

        # Allow deleting the master file separately (admin only)
        with st.expander('Master controls'):
            if st.button('Delete master file'):
                confirm = st.checkbox('Confirm delete master (this cannot be undone)')
                if confirm:
                    try:
                        if MASTER_PATH.exists():
                            MASTER_PATH.unlink()
                        st.success('Master file deleted')
                        # log deletion
                        ts = datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
                        log_row = {
                            'timestamp_utc': ts,
                            'original_filename': 'admin_delete_master',
                            'saved_path': '',
                            'results_path': str(MASTER_PATH.as_posix()),
                            'rows_processed': 0,
                            'wo_volume_total': 0,
                            'overwritten_weeks': 'deleted_master'
                        }
                        if LOG_PATH.exists():
                            log_df = pd.read_csv(LOG_PATH)
                            log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
                        else:
                            log_df = pd.DataFrame([log_row])
                        log_df.to_csv(LOG_PATH, index=False)
                        try:
                            st.experimental_rerun()
                        except Exception:
                            st.info('Please refresh the page to complete the deletion.')
                    except Exception as e:
                        st.error(f'Failed to delete master: {e}')

    # Save current master to History
    st.markdown('')
    st.write('Save current master to History (give it a name)')
    history_name = st.text_input('History name', value=f"Master_{datetime.utcnow().strftime('%Y%m%d')}", key='history_name')
    if st.button('Save master to History'):
        try:
            if MASTER_PATH.exists():
                ts2 = datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
                safe = "".join(c for c in history_name if c.isalnum() or c in (' ', '-', '_')).rstrip()
                fname = f"{safe}_{ts2}.csv"
                dest = HISTORY_DIR / fname
                shutil.copy2(MASTER_PATH, dest)
                # append to history index
                hist_row = {
                    'timestamp_utc': ts2,
                    'name': history_name,
                    'file_path': str(dest.as_posix()),
                    'rows': '',
                    'wo_volume_total': ''
                }
                try:
                    dfh = pd.read_csv(HISTORY_INDEX)
                    dfh = pd.concat([dfh, pd.DataFrame([hist_row])], ignore_index=True)
                except Exception:
                    dfh = pd.DataFrame([hist_row])
                dfh.to_csv(HISTORY_INDEX, index=False)
                st.success(f"Master saved to history: {fname}")
            else:
                st.error('No master file exists to save')
        except Exception as e:
            st.error(f'Failed to save master to history: {e}')

    uploaded = st.file_uploader("Upload .xlsx file", type=["xlsx"], key='uploader')

    if uploaded is not None:
        with st.spinner("Processing upload..."):
            try:
                # Save original upload with timestamp
                ts = datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
                original_name = Path(uploaded.name).name
                safe_name = f"{ts}_{original_name}"
                saved_path = DATA_DIR / safe_name
                with open(saved_path, "wb") as f:
                    f.write(uploaded.getvalue())

                # Process using the saved file path and store in session
                df_weekly = process_file(saved_path)
                # Determine years covered by the raw sheet
                try:
                    raw = load_workbook(saved_path)
                    rawn = normalize_df(raw)
                    years_covered = sorted(set(rawn['date_completed'].dt.year.dropna().astype(int).tolist()))
                except Exception:
                    years_covered = []
                st.session_state['latest_df'] = df_weekly
                st.session_state['latest_saved_path'] = str(saved_path)
                st.session_state['latest_ts'] = ts
                st.session_state['latest_original_name'] = original_name
                st.session_state['latest_years'] = years_covered

                # Load master if exists
                master_df = load_master()

                # Ensure Week columns are ints for reliable comparisons
                df_weekly['Week'] = df_weekly['Week'].astype(int)

                # Determine overlapping weeks where both master and new have WO Volume > 0
                new_nonzero = set(df_weekly.loc[df_weekly['WO Volume'] > 0, 'Week'].astype(int).tolist())
                overlapping = []
                if master_df is not None:
                    master_nonzero = set(master_df.loc[master_df['WO Volume'] > 0, 'Week'].astype(int).tolist())
                    overlapping = sorted(list(new_nonzero.intersection(master_nonzero)))

                st.session_state['latest_master_exists'] = master_df is not None
                st.session_state['latest_overlapping'] = overlapping

            except Exception as e:
                st.error(f"Processing failed: {e}")
                raise

        st.success("Processing complete — review merge options below")

        st.subheader("Latest Results (Preview)")
        df_weekly = st.session_state.get('latest_df')
        st.dataframe(df_weekly.style.format({
            'Weekly Return %': '{:.2f}%',
            '% ≤24 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
            '% ≤48 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
        }))

        # Merge controls (same logic as before)
        master_df = load_master()
        overlapping = st.session_state.get('latest_overlapping', [])
        if master_df is None:
            st.info('No existing master history found. Saving this upload will create master history for Weeks 1-52.')
            if st.button('Save as master and finalize'):
                # Save results excel and master
                ts = st.session_state['latest_ts']
                results_name = f"{ts}_results.xlsx"
                results_path = DATA_DIR / results_name
                df_weekly.to_excel(results_path, index=False, sheet_name='Weeks 1-52', engine='openpyxl')

                # Save master
                df_weekly.to_csv(MASTER_PATH, index=False)

                # Update uploads log
                log_row = {
                    'timestamp_utc': ts,
                    'original_filename': st.session_state['latest_original_name'],
                    'saved_path': st.session_state['latest_saved_path'],
                    'results_path': str(results_path.as_posix()),
                    'rows_processed': int(getattr(df_weekly, 'shape', (0,0))[0]),
                    'wo_volume_total': int(df_weekly['WO Volume'].sum()) if 'WO Volume' in df_weekly.columns else 0,
                    'overwritten_weeks': '',
                    'years': ','.join(map(str, years_covered)) if years_covered else ''
                }
                if LOG_PATH.exists():
                    log_df = pd.read_csv(LOG_PATH)
                    log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
                else:
                    log_df = pd.DataFrame([log_row])
                log_df.to_csv(LOG_PATH, index=False)
                st.success('Master history created and upload logged')
        else:
            if not overlapping:
                st.info('No overlapping weeks found. Saving will append new weeks (non-zero) into master.')
                if st.button('Merge and save (no overlaps)'):
                    # merge robustly across Weeks 1..52: keep master unless new has WO Volume>0
                    m = master_df.set_index('Week')
                    n = df_weekly.set_index('Week')
                    # ensure index contains all weeks
                    for wk in range(1, 53):
                        if wk not in m.index:
                            # create empty row from n or default
                            if wk in n.index:
                                m.loc[wk] = n.loc[wk]
                            else:
                                m.loc[wk] = [wk] + [pd.NA] * (len(master_df.columns) - 1)
                    # apply replacements where new has volume > 0
                    for wk in n.index:
                        try:
                            if int(n.loc[wk, 'WO Volume']) > 0:
                                m.loc[wk] = n.loc[wk]
                        except Exception:
                            m.loc[wk] = n.loc[wk]
                    merged = m.reset_index()
                    # ensure Week column first and sorted
                    merged = merged.sort_values('Week')
                    merged.to_csv(MASTER_PATH, index=False)

                    # save results file
                    ts = st.session_state['latest_ts']
                    results_name = f"{ts}_results.xlsx"
                    results_path = DATA_DIR / results_name
                    df_weekly.to_excel(results_path, index=False, sheet_name='Weeks 1-52', engine='openpyxl')

                    # log
                    log_row = {
                        'timestamp_utc': ts,
                        'original_filename': st.session_state['latest_original_name'],
                        'saved_path': st.session_state['latest_saved_path'],
                        'results_path': str(results_path.as_posix()),
                        'rows_processed': int(getattr(df_weekly, 'shape', (0,0))[0]),
                        'wo_volume_total': int(df_weekly['WO Volume'].sum()) if 'WO Volume' in df_weekly.columns else 0,
                        'overwritten_weeks': ''
                    }
                    if LOG_PATH.exists():
                        log_df = pd.read_csv(LOG_PATH)
                        log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
                    else:
                        log_df = pd.DataFrame([log_row])
                    log_df.to_csv(LOG_PATH, index=False)
                    # save per-year cache so year tabs pick it up
                    for y in years_covered:
                        try:
                            # save a copy of results to per-year result file if needed
                            pass
                        except Exception:
                            pass

                    st.success('Merged and saved')
            else:
                st.warning(f'Overlapping weeks detected: {overlapping}')
                st.markdown('Select which overlapping weeks to OVERWRITE in the master history with the new upload. Unselected overlapping weeks will keep the existing master values.')
                overwrite_weeks = st.multiselect('Weeks to overwrite', overlapping, default=[])
                if st.checkbox('Overwrite all overlapping weeks'):
                    overwrite_weeks = overlapping
                if st.button('Apply merge & save'):
                    m = master_df.set_index('Week')
                    n = df_weekly.set_index('Week')
                    # ensure master has all weeks
                    for wk in range(1, 53):
                        if wk not in m.index:
                            if wk in n.index:
                                m.loc[wk] = n.loc[wk]
                            else:
                                m.loc[wk] = [wk] + [pd.NA] * (len(master_df.columns) - 1)
                    # replace selected weeks
                    for wk in overwrite_weeks:
                        if wk in n.index:
                            m.loc[wk] = n.loc[wk]
                    # Always bring in new weeks where WO Volume > 0,
                    # EXCEPT overlapping weeks the user chose NOT to overwrite.
                    for wk in n.index:
                        if wk in overlapping and wk not in overwrite_weeks:
                            continue  # preserve master for overlapping weeks not selected

                        try:
                            if int(n.loc[wk, 'WO Volume']) > 0:
                                m.loc[wk] = n.loc[wk]
                        except Exception:
                            # if parsing WO Volume fails, still prefer new row
                            m.loc[wk] = n.loc[wk]
                    merged = m.reset_index()
                    merged = merged.sort_values('Week')
                    merged.to_csv(MASTER_PATH, index=False)

                    # save results file
                    ts = st.session_state['latest_ts']
                    results_name = f"{ts}_results.xlsx"
                    results_path = DATA_DIR / results_name
                    df_weekly.to_excel(results_path, index=False, sheet_name='Weeks 1-52', engine='openpyxl')

                    # log with overwritten weeks listed
                    log_row = {
                        'timestamp_utc': ts,
                        'original_filename': st.session_state['latest_original_name'],
                        'saved_path': st.session_state['latest_saved_path'],
                        'results_path': str(results_path.as_posix()),
                        'rows_processed': int(getattr(df_weekly, 'shape', (0,0))[0]),
                        'wo_volume_total': int(df_weekly['WO Volume'].sum()) if 'WO Volume' in df_weekly.columns else 0,
                        'overwritten_weeks': ','.join(map(str, overwrite_weeks))
                    }
                    if LOG_PATH.exists():
                        log_df = pd.read_csv(LOG_PATH)
                        log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
                    else:
                        log_df = pd.DataFrame([log_row])
                    log_df.to_csv(LOG_PATH, index=False)

                    st.success('Merge applied, master updated, and upload logged')
                    # reload master for display
                    master_df = load_master()
                    if master_df is not None:
                        st.subheader('Master (post-merge)')
                        st.dataframe(master_df.style.format({
                            'Weekly Return %': '{:.2f}%',
                            '% ≤24 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
                            '% ≤48 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
                        }))

    st.markdown('---')

    # Show upload history and allow selecting past entries
    st.subheader("Upload History")
    if LOG_PATH.exists():
        history = pd.read_csv(LOG_PATH)
        # Display history table
        st.dataframe(history)

        # Danger zone: clear all saved data
        with st.expander("Danger Zone — Clear all saved data"):
            st.warning("This will permanently delete all saved uploads, results, master history, and logs.")
            confirm = st.checkbox("I understand this will delete ALL files in the data folder and cannot be undone.")
            if confirm and st.button('Clear all history and start over'):
                # Delete all files/folders inside DATA_DIR
                try:
                    for item in DATA_DIR.iterdir():
                        if item.is_dir():
                            shutil.rmtree(item)
                        else:
                            item.unlink()
                    DATA_DIR.mkdir(parents=True, exist_ok=True)
                    st.success('All saved data cleared.')
                    # Try to trigger a rerun by updating query params; if unavailable, ask user to refresh
                    try:
                        st.experimental_set_query_params(cleared=ts)
                        st.stop()
                    except Exception:
                        st.info('Please refresh the page to complete the reset.')
                except Exception as e:
                    st.error(f'Failed to clear data: {e}')

        sel_idx = st.selectbox('Select entry to download', history.index.tolist())
        if sel_idx is not None:
            row = history.loc[sel_idx]
            if st.button('Download selected original'):
                p = Path(row['saved_path'])
                if p.exists():
                    with open(p, 'rb') as f:
                        st.download_button(label='Download', data=f.read(), file_name=p.name)
                else:
                    st.error('Original file not found')
            if st.button('Download selected results'):
                p = Path(row['results_path'])
                if p.exists():
                    with open(p, 'rb') as f:
                        st.download_button(label='Download', data=f.read(), file_name=p.name)
                else:
                    st.error('Results file not found')
    else:
        st.info('No uploads logged yet')

    # Master History (saved master snapshots)
    st.markdown('---')
    st.subheader('Master History (saved snapshots)')
    if HISTORY_INDEX.exists():
        hh = pd.read_csv(HISTORY_INDEX)
        if hh.empty:
            st.info('No master snapshots saved yet')
        else:
            st.dataframe(hh)
            sel = st.selectbox('Select snapshot', hh.index.tolist())
            if sel is not None:
                row = hh.loc[sel]
                p = Path(row.get('file_path', ''))
                if p.exists():
                    if st.button('Download selected master snapshot'):
                        with open(p, 'rb') as f:
                            st.download_button(label='Download', data=f.read(), file_name=p.name)
                    if st.button('Delete selected master snapshot'):
                        try:
                            p.unlink()
                            hh = hh.drop(index=sel).reset_index(drop=True)
                            hh.to_csv(HISTORY_INDEX, index=False)
                            st.success('Snapshot deleted')
                        except Exception as e:
                            st.error(f'Failed to delete snapshot: {e}')
                else:
                    st.error('Snapshot file not found on disk')
    else:
        st.info('No master snapshots saved yet')
import os
from pathlib import Path
from datetime import datetime

import streamlit as st
import pandas as pd
from percent_return.processor import process_file
from io import BytesIO
import shutil


st.set_page_config(page_title="% of Return — iBot Metrics", layout="wide")

# Data directory for saved uploads and results
DATA_DIR = Path("data")
DATA_DIR.mkdir(parents=True, exist_ok=True)
LOG_PATH = DATA_DIR / "uploads_log.csv"

st.title("% of Return — iBot Metrics Calculator")

st.markdown("Upload an Excel file exported from HxGN EAM / APM. The app follows the exact logic for same-day returns and weekly rollup.")

uploaded = st.file_uploader("Upload .xlsx file", type=["xlsx"])

if uploaded is not None:
    with st.spinner("Processing upload..."):
        try:
            # Save original upload with timestamp
            ts = datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
            original_name = Path(uploaded.name).name
            safe_name = f"{ts}_{original_name}"
            saved_path = DATA_DIR / safe_name
            with open(saved_path, "wb") as f:
                f.write(uploaded.getvalue())

            # Process using the saved file path and store in session
            df_weekly = process_file(saved_path)
            st.session_state['latest_df'] = df_weekly
            st.session_state['latest_saved_path'] = str(saved_path)
            st.session_state['latest_ts'] = ts
            st.session_state['latest_original_name'] = original_name

            # Load master if exists
            master_path = DATA_DIR / 'master_weeks.csv'
            master_df = pd.read_csv(master_path) if master_path.exists() else None

            # Ensure Week columns are ints for reliable comparisons
            df_weekly['Week'] = df_weekly['Week'].astype(int)
            if master_df is not None:
                master_df['Week'] = master_df['Week'].astype(int)

            # Determine overlapping weeks where both master and new have WO Volume > 0
            new_nonzero = set(df_weekly.loc[df_weekly['WO Volume'] > 0, 'Week'].astype(int).tolist())
            overlapping = []
            if master_df is not None:
                master_nonzero = set(master_df.loc[master_df['WO Volume'] > 0, 'Week'].astype(int).tolist())
                overlapping = sorted(list(new_nonzero.intersection(master_nonzero)))

            st.session_state['latest_master_exists'] = master_df is not None
            st.session_state['latest_overlapping'] = overlapping

        except Exception as e:
            st.error(f"Processing failed: {e}")
            raise

    st.success("Processing complete — review merge options below")

    st.subheader("Latest Results (Preview)")
    df_weekly = st.session_state.get('latest_df')
    st.dataframe(df_weekly.style.format({
        'Weekly Return %': '{:.2f}%',
        '% ≤24 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
        '% ≤48 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
    }))

    master_path = DATA_DIR / 'master_weeks.csv'
    master_df = pd.read_csv(master_path) if master_path.exists() else None
    if master_df is not None and 'Week' in master_df.columns:
        try:
            master_df['Week'] = master_df['Week'].astype(int)
        except Exception:
            # fallback: coerce via to_numeric then int
            master_df['Week'] = pd.to_numeric(master_df['Week'], errors='coerce').fillna(0).astype(int)

    # Merge controls
    st.subheader('Merge new results into master history')
    overlapping = st.session_state.get('latest_overlapping', [])
    if master_df is None:
        st.info('No existing master history found. Saving this upload will create master history for Weeks 1-52.')
        if st.button('Save as master and finalize'):
            # Save results excel and master
            ts = st.session_state['latest_ts']
            results_name = f"{ts}_results.xlsx"
            results_path = DATA_DIR / results_name
            df_weekly.to_excel(results_path, index=False, sheet_name='Weeks 1-52', engine='openpyxl')

            # Save master
            df_weekly.to_csv(master_path, index=False)

            # Update uploads log
            log_row = {
                'timestamp_utc': ts,
                'original_filename': st.session_state['latest_original_name'],
                'saved_path': st.session_state['latest_saved_path'],
                'results_path': str(results_path.as_posix()),
                'rows_processed': int(getattr(df_weekly, 'shape', (0,0))[0]),
                'wo_volume_total': int(df_weekly['WO Volume'].sum()) if 'WO Volume' in df_weekly.columns else 0,
                'overwritten_weeks': ''
            }
            if LOG_PATH.exists():
                log_df = pd.read_csv(LOG_PATH)
                log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
            else:
                log_df = pd.DataFrame([log_row])
            log_df.to_csv(LOG_PATH, index=False)
            st.success('Master history created and upload logged')
    else:
        if not overlapping:
            st.info('No overlapping weeks found. Saving will append new weeks (non-zero) into master.')
            if st.button('Merge and save (no overlaps)'):
                # merge robustly across Weeks 1..52: keep master unless new has WO Volume>0
                m = master_df.set_index('Week')
                n = df_weekly.set_index('Week')
                # ensure index contains all weeks
                for wk in range(1, 53):
                    if wk not in m.index:
                        # create empty row from n or default
                        if wk in n.index:
                            m.loc[wk] = n.loc[wk]
                        else:
                            m.loc[wk] = [wk] + [pd.NA] * (len(master_df.columns) - 1)
                # apply replacements where new has volume > 0
                for wk in n.index:
                    try:
                        if int(n.loc[wk, 'WO Volume']) > 0:
                            m.loc[wk] = n.loc[wk]
                    except Exception:
                        m.loc[wk] = n.loc[wk]
                merged = m.reset_index()
                # ensure Week column first and sorted
                merged = merged.sort_values('Week')
                merged.to_csv(master_path, index=False)

                # save results file
                ts = st.session_state['latest_ts']
                results_name = f"{ts}_results.xlsx"
                results_path = DATA_DIR / results_name
                df_weekly.to_excel(results_path, index=False, sheet_name='Weeks 1-52', engine='openpyxl')

                # log
                log_row = {
                    'timestamp_utc': ts,
                    'original_filename': st.session_state['latest_original_name'],
                    'saved_path': st.session_state['latest_saved_path'],
                    'results_path': str(results_path.as_posix()),
                    'rows_processed': int(getattr(df_weekly, 'shape', (0,0))[0]),
                    'wo_volume_total': int(df_weekly['WO Volume'].sum()) if 'WO Volume' in df_weekly.columns else 0,
                    'overwritten_weeks': ''
                }
                if LOG_PATH.exists():
                    log_df = pd.read_csv(LOG_PATH)
                    log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
                else:
                    log_df = pd.DataFrame([log_row])
                log_df.to_csv(LOG_PATH, index=False)

                st.success('Merged and saved')
        else:
            st.warning(f'Overlapping weeks detected: {overlapping}')
            st.markdown('Select which overlapping weeks to OVERWRITE in the master history with the new upload. Unselected overlapping weeks will keep the existing master values.')
            overwrite_weeks = st.multiselect('Weeks to overwrite', overlapping, default=[])
            if st.checkbox('Overwrite all overlapping weeks'):
                overwrite_weeks = overlapping
            if st.button('Apply merge & save'):
                m = master_df.set_index('Week')
                n = df_weekly.set_index('Week')
                # ensure master has all weeks
                for wk in range(1, 53):
                    if wk not in m.index:
                        if wk in n.index:
                            m.loc[wk] = n.loc[wk]
                        else:
                            m.loc[wk] = [wk] + [pd.NA] * (len(master_df.columns) - 1)
                # replace selected weeks
                for wk in overwrite_weeks:
                    if wk in n.index:
                        m.loc[wk] = n.loc[wk]
                # Always bring in new weeks where WO Volume > 0,
                # EXCEPT overlapping weeks the user chose NOT to overwrite.
                for wk in n.index:
                    if wk in overlapping and wk not in overwrite_weeks:
                        continue  # preserve master for overlapping weeks not selected

                    try:
                        if int(n.loc[wk, 'WO Volume']) > 0:
                            m.loc[wk] = n.loc[wk]
                    except Exception:
                        # if parsing WO Volume fails, still prefer new row
                        m.loc[wk] = n.loc[wk]
                merged = m.reset_index()
                merged = merged.sort_values('Week')
                merged.to_csv(master_path, index=False)

                # save results file
                ts = st.session_state['latest_ts']
                results_name = f"{ts}_results.xlsx"
                results_path = DATA_DIR / results_name
                df_weekly.to_excel(results_path, index=False, sheet_name='Weeks 1-52', engine='openpyxl')

                # log with overwritten weeks listed
                log_row = {
                    'timestamp_utc': ts,
                    'original_filename': st.session_state['latest_original_name'],
                    'saved_path': st.session_state['latest_saved_path'],
                    'results_path': str(results_path.as_posix()),
                    'rows_processed': int(getattr(df_weekly, 'shape', (0,0))[0]),
                    'wo_volume_total': int(df_weekly['WO Volume'].sum()) if 'WO Volume' in df_weekly.columns else 0,
                    'overwritten_weeks': ','.join(map(str, overwrite_weeks))
                }
                if LOG_PATH.exists():
                    log_df = pd.read_csv(LOG_PATH)
                    log_df = pd.concat([log_df, pd.DataFrame([log_row])], ignore_index=True)
                else:
                    log_df = pd.DataFrame([log_row])
                log_df.to_csv(LOG_PATH, index=False)

                st.success('Merge applied, master updated, and upload logged')
                # reload master for display
                master_df = pd.read_csv(master_path)
                master_df['Week'] = master_df['Week'].astype(int)
                st.subheader('Master (post-merge)')
                st.dataframe(master_df.style.format({
                    'Weekly Return %': '{:.2f}%',
                    '% ≤24 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
                    '% ≤48 hrs': lambda v: (f"{v:.2f}%" if pd.notna(v) else ''),
                }))

    st.markdown('---')

    # Show upload history and allow selecting past entries
    st.subheader("Upload History")
    if LOG_PATH.exists():
        history = pd.read_csv(LOG_PATH)
        # Display history table
        st.dataframe(history)

        # Danger zone: clear all saved data
        with st.expander("Danger Zone — Clear all saved data"):
            st.warning("This will permanently delete all saved uploads, results, master history, and logs.")
            confirm = st.checkbox("I understand this will delete ALL files in the data folder and cannot be undone.")
            if confirm and st.button('Clear all history and start over'):
                # Delete all files/folders inside DATA_DIR
                try:
                    for item in DATA_DIR.iterdir():
                        if item.is_dir():
                            shutil.rmtree(item)
                        else:
                            item.unlink()
                    DATA_DIR.mkdir(parents=True, exist_ok=True)
                    st.success('All saved data cleared.')
                    # Try to trigger a rerun by updating query params; if unavailable, ask user to refresh
                    try:
                        st.experimental_set_query_params(cleared=ts)
                        st.stop()
                    except Exception:
                        st.info('Please refresh the page to complete the reset.')
                except Exception as e:
                    st.error(f'Failed to clear data: {e}')

        sel_idx = st.selectbox('Select entry to download', history.index.tolist())
        if sel_idx is not None:
            row = history.loc[sel_idx]
            if st.button('Download selected original'):
                p = Path(row['saved_path'])
                if p.exists():
                    with open(p, 'rb') as f:
                        st.download_button(label='Download', data=f.read(), file_name=p.name)
                else:
                    st.error('Original file not found')
            if st.button('Download selected results'):
                p = Path(row['results_path'])
                if p.exists():
                    with open(p, 'rb') as f:
                        st.download_button(label='Download', data=f.read(), file_name=p.name)
                else:
                    st.error('Results file not found')
    else:
        st.info('No uploads logged yet')
