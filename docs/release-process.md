# Release Process

The first public distribution should be a portable unsigned zip.

## Publish

```powershell
dotnet publish src/FlexSliceCompanion.Windows/FlexSliceCompanion.Windows.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true
```

Package:

```text
FlexSliceCompanion.exe
FlexSliceCompanion.dll
appsettings.example.json
README.md
docs/
```

## Notes

- Do not require admin rights.
- Do not install or modify FlexRadio DAX or CAT drivers.
- Expect a SmartScreen warning while unsigned.
- Expect a Windows Firewall prompt when local TCP listeners are enabled.
