# FlexSliceCompanion

Windows-first companion utility for FlexRadio FLEX-6000 and FLEX-8000 radios.

The first MVP targets:

- Discover and connect to FlexRadio radios.
- Display slice letter, frequency, mode, DAX channel, active state, and TX state.
- Detect and normalize DAXv1 and DAXv2 Windows audio endpoints.
- Ignore legacy DAX reserved devices for third-party app assignment.
- Launch WSJT-X with a per-slice settings directory.
- Expose a per-slice local CAT/HRD-style TCP port.
- Keep frequency and mode synchronized between WSJT-X and the Flex slice.

## Current Status

This repo is scaffolded around the attached Windows-first project plan. The core logic is intentionally separated from WPF and FlexLib so the shared pieces can later be reused by a SmartSDR TCP/UDP client or an Avalonia front end.

The local OpenClaw machine does not currently have the .NET SDK installed, so build/test verification should be run from Windows with .NET 8 or newer.

## Layout

```text
src/
  FlexSliceCompanion.Core/          Shared models, DAX detection, CAT parser, config
  FlexSliceCompanion.Windows/       WPF shell and Windows services
  FlexSliceCompanion.FlexLib/       Windows-first FlexLib adapter placeholder
  FlexSliceCompanion.SmartSdrApi/   Future cross-platform TCP/UDP API adapter
  FlexSliceCompanion.Plugins.Wsjtx/ WSJT-X launch/config helpers
tests/
  FlexSliceCompanion.Tests/         xUnit tests for core MVP behavior
docs/
  architecture.md
  dax-compatibility.md
  cat-protocol.md
  wsjtx-integration.md
  release-process.md
```

## Build

From Windows:

```powershell
dotnet restore
dotnet test
dotnet build src/FlexSliceCompanion.Windows/FlexSliceCompanion.Windows.csproj
```

## GitHub Actions

`.github/workflows/windows-build.yml` runs on pushes to `main`, pull requests targeting `main`, and manual dispatches.

The workflow:

- Restores the solution on `windows-latest`.
- Runs the xUnit test suite in Release.
- Publishes a self-contained `win-x64` portable build.
- Uploads the portable build as the `FlexSliceCompanion-win-x64` artifact.

Portable release:

```powershell
dotnet publish src/FlexSliceCompanion.Windows/FlexSliceCompanion.Windows.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true
```

## DAX Compatibility

The DAX detector supports both legacy and new endpoint names:

- `DAX Audio RX 1`
- `DAX Audio TX`
- `DAX Mic Audio`
- `DAX IQ RX 1`
- `DAX RESERVED AUDIO RX 1`
- `DAX RX 1 (FlexRadio DAX)`
- `DAX TX (FlexRadio DAX)`
- `DAX Mic (FlexRadio DAX)`
- `DAX IQ 1 (FlexRadio DAX)`

Reserved DAXv1 devices are preserved in the normalized scan results, but filtered out by `GetAssignableEndpoints` for digital app audio assignment.
