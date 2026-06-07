# Testing

This project has three test layers:

1. Automated CI tests on GitHub's hosted Windows runner.
2. Local Windows packaging tests.
3. Manual FlexRadio, DAX, CAT, and WSJT-X hardware validation.

## GitHub Actions Test

Every push to `main` runs `.github/workflows/windows-build.yml`.

The workflow:

- Restores `FlexSliceCompanion.sln`.
- Runs the xUnit tests in Release.
- Publishes a self-contained `win-x64` WPF build.
- Uploads two artifacts:
  - `FlexSliceCompanion-win-x64`
  - `FlexSliceCompanion-win-x64-zip`

To download a successful build:

1. Open the repository on GitHub.
2. Go to **Actions**.
3. Open the latest successful **Windows Build** run.
4. Download `FlexSliceCompanion-win-x64-zip`.
5. Extract the zip on Windows.
6. Run `FlexSliceCompanion.exe`.

The app is unsigned, so Windows may show a SmartScreen warning.

## Local Windows Test

Use PowerShell from the repository root:

```powershell
dotnet restore FlexSliceCompanion.sln
dotnet test FlexSliceCompanion.sln --configuration Release
./scripts/package-windows.ps1
```

The local package script creates:

```text
publish/FlexSliceCompanion-win-x64/
publish/FlexSliceCompanion-win-x64.zip
```

## Manual Smoke Test Without Hardware

1. Start `FlexSliceCompanion.exe`.
2. Confirm the app opens without errors.
3. Confirm the demo FLEX radio fallback appears if no radio is discovered.
4. Click DAX refresh and verify no crash when SmartSDR DAX is not running.
5. Confirm logs are written under:

```text
%LOCALAPPDATA%/FlexSliceCompanion/logs
```

## Manual FlexRadio Test

Prerequisites:

- Windows 10 or newer.
- SmartSDR installed.
- SmartSDR DAX running.
- WSJT-X installed.
- FLEX-6000 or FLEX-8000 radio reachable on the local network.

Checklist:

1. Start SmartSDR and connect to the radio.
2. Enable DAX on at least one slice.
3. Start FlexSliceCompanion.
4. Confirm the radio and slice appear.
5. Confirm slice frequency, mode, DAX channel, active state, and TX state look correct.
6. Refresh DAX endpoints.
7. Confirm assignable DAX RX/TX endpoints appear and reserved DAXv1 endpoints are not selected for app assignment.
8. Launch WSJT-X for slice A.
9. Confirm WSJT-X opens with a slice-specific settings directory.
10. Confirm FlexSliceCompanion creates a companion settings file under:

```text
%APPDATA%/FlexSliceCompanion/wsjtx/
```

11. In WSJT-X, verify CAT points to the local CAT port for that slice.
12. Confirm CAT frequency and mode reads work.
13. Change slice frequency in SmartSDR and confirm the app state updates.
14. Close WSJT-X and relaunch it from FlexSliceCompanion to verify repeated launch cleanup.

## Known Validation Gap

The hosted GitHub runner cannot see local FlexRadio hardware, SmartSDR, DAX devices, or WSJT-X installs. It proves the Windows build and automated tests. Final radio behavior still needs a real Windows station or a self-hosted Windows runner on the radio network.
