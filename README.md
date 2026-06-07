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

This repo implements a first MVP around the attached Windows-first project plan. The core logic is intentionally separated from WPF and FlexLib so the shared pieces can later be reused by another front end.

The app currently supports:

- FlexLib adapter boundary for the preferred Windows integration path.
- Direct SmartSDR UDP discovery and TCP command/status parsing.
- Demo radio fallback for development without hardware.
- DAXv1/DAXv2 endpoint detection and per-slice DAX selection.
- Per-slice CAT server management.
- WSJT-X per-slice launch directories and companion settings files.
- Windows logging under `%LOCALAPPDATA%/FlexSliceCompanion/logs`.

Hardware validation still needs a Windows machine with SmartSDR/FlexRadio available.

## Layout

```text
src/
  FlexSliceCompanion.Core/          Shared models, DAX detection, CAT parser, config
  FlexSliceCompanion.Windows/       WPF shell and Windows services
  FlexSliceCompanion.FlexLib/       Windows-first FlexLib adapter boundary
  FlexSliceCompanion.SmartSdrApi/   Direct SmartSDR TCP/UDP discovery and command path
  FlexSliceCompanion.Plugins.Wsjtx/ WSJT-X launch/config helpers
tests/
  FlexSliceCompanion.Tests/         xUnit tests for core MVP behavior
docs/
  architecture.md
  dax-compatibility.md
  cat-protocol.md
  wsjtx-integration.md
  testing.md
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
- Uploads a zip archive as the `FlexSliceCompanion-win-x64-zip` artifact.

The latest successful build can be downloaded from the workflow run artifacts in GitHub Actions.

## Testing

See [docs/testing.md](docs/testing.md) for the full test checklist.

Fast checks:

```powershell
dotnet restore FlexSliceCompanion.sln
dotnet test FlexSliceCompanion.sln --configuration Release
./scripts/package-windows.ps1
```

Manual hardware validation needs Windows with SmartSDR, DAX, WSJT-X, and a reachable FLEX radio.

Portable release:

```powershell
./scripts/package-windows.ps1
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
