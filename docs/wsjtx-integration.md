# WSJT-X Integration

WSJT-X is the first digital-mode app target.

## MVP Behavior

- Detect or configure the WSJT-X executable path.
- Create one settings directory per slice.
- Start WSJT-X with a slice-specific rig name and config path.
- Point rig control at the matching local CAT port.
- Select the matching DAX RX and TX endpoints.

## Settings Layout

```text
%APPDATA%/FlexSliceCompanion/wsjtx/
  slice-A/
  slice-B/
  slice-C/
  slice-D/
```

The first pass creates and launches per-slice folders. Automated editing of WSJT-X config files should be added cautiously because the file format can change.
