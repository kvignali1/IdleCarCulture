# Percent of Return App (iOS)

A new iOS app to calculate/display percent of return. This repository contains the Xcode project (Swift/SwiftUI) and CI configuration.

Quick start

1. Clone this workspace to your Mac (Xcode required).
2. Create a remote GitHub repository (private) and add it as `origin`:

```bash
# create remote via GitHub UI or CLI (example with gh):
gh repo create <owner>/PercentOfReturnApp --private --source=. --remote=origin

# or create repo on GitHub, then on your machine:
git remote add origin git@github.com:<owner>/PercentOfReturnApp.git
git push -u origin main
```

If you want me to create the GitHub repo for you, share a Personal Access Token with `repo` scope or run `gh auth login` locally and I can provide the `gh` command to run.

What I added locally

- `.gitignore` — Xcode-friendly ignore rules
- `.gitattributes`
- `README.md`

Next steps I can take

- Create a SwiftUI Xcode project scaffold and add it to this repo
- Add GitHub Actions macOS workflow for CI
- Create the remote GitHub repository (requires auth)
# % of Return — iBot Metrics Calculator

Simple Streamlit app to compute same-day return metrics and weekly rollups from an HxGN EAM (APM) Excel export following the provided master prompt logic.

Quick start

1. Create a virtualenv and install:

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

2. Run the app:

```powershell
streamlit run app.py
```

3. Upload your .xlsx export and download the Weeks 1–52 table.

Notes

- The app assigns ISO week from `date_completed` and uses `date_reported` as opened date (per spec).
- Weeks are reported 1–52. Week 53 (if present) is folded into week 52 to match the requested output range.
