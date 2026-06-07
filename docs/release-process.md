# Release Process

The first public distribution should be a portable unsigned zip.

## Publish

GitHub Actions publishes a portable Windows artifact on every push to `main`.

To use the hosted build:

1. Open the latest successful **Windows Build** run in GitHub Actions.
2. Download `FlexSliceCompanion-win-x64-zip`.
3. Extract the zip on Windows.
4. Run `FlexSliceCompanion.exe`.

To build locally on Windows:

```powershell
./scripts/package-windows.ps1
```

Package:

```text
FlexSliceCompanion.exe
appsettings.example.json
README.md
LICENSE
docs/
```

## Notes

- Do not require admin rights.
- Do not install or modify FlexRadio DAX or CAT drivers.
- Expect a SmartScreen warning while unsigned.
- Expect a Windows Firewall prompt when local TCP listeners are enabled.
- See `docs/testing.md` for CI, local, and hardware validation steps.
