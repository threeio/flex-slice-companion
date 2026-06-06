# Architecture

FlexSliceCompanion starts Windows-first, but the radio-control logic is kept behind interfaces so later macOS/Linux work can use the SmartSDR TCP/UDP API instead of FlexLib.

## Layers

```text
WPF UI
  -> ViewModels
  -> Core services and interfaces
  -> FlexLib adapter on Windows
  -> SmartSDR TCP/UDP adapter in the future
```

## Core Contracts

- `IRadioClient` handles discovery, connection, slice updates, and slice commands.
- `SliceState` is the UI/app-facing slice model.
- `DaxEndpointDetector` normalizes Windows audio endpoint names into `DaxEndpoint`.
- `IExternalRadioApp` abstracts WSJT-X, JTDX, JS8Call, and CW Skimmer launchers.
- `CatServer` exposes a local TCP control path per managed slice.

## MVP Scope

The MVP should avoid deep automation until the basics are stable:

1. Start the WPF app.
2. Discover radios through FlexLib.
3. Connect to one radio.
4. Display slices.
5. Detect DAX devices.
6. Launch WSJT-X for slice A.
7. Expose CAT on `localhost:5101`.
8. Sync frequency both ways.
