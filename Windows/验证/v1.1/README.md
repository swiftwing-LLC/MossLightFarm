# Windows 1.1 validation

Date: 2026-09-24

- Source build: 260 checks passed. Includes direct seed selection, planting, harvesting, land purchase, no plot-grid hit targets, and English translation coverage.
- Extracted v1.1 installer: 259 checks passed. The packaged build omits source-only translation-literal scanning.
- Extracted maintenance v1.1 zip: 13 checks passed, including all building tiers at 60 seconds, unlimited spending, isolated save path, and production pause while construction runs.
- `english-untranslated.txt` is empty.
- `seed-page.png` shows the 1200x800 fields page. The wallpaper fields remain in the background; the panel contains only seeds and harvest/plant/buy actions.
- `seed-page-maintenance.png` confirms the infinity balance and the separately labeled maintenance build.

The v1.1 installer is in `../最新版`. The maintenance bundle is in `../维护版`. The former v1.0 installer remains under `../历史版本` for rollback. Existing user save data was not reset.
