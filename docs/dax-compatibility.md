# DAX Compatibility

SmartSDR v4.2.18 introduced DAXv2. The app must support both DAXv1 and DAXv2 device names because users may have old devices, new devices, or remnants of both.

## DAXv1 Names

```text
DAX Audio RX 1
DAX Audio TX
DAX Mic Audio
DAX IQ RX 1
DAX RESERVED AUDIO RX 1
DAX RESERVED AUDIO TX
DAX RESERVED MIC AUDIO
DAX RESERVED IQ RX 1
```

## DAXv2 Names

```text
DAX RX 1 (FlexRadio DAX)
DAX TX (FlexRadio DAX)
DAX Mic (FlexRadio DAX)
DAX IQ 1 (FlexRadio DAX)
```

## Rules

- Preserve reserved DAXv1 devices in scan results so diagnostics are honest.
- Filter reserved DAXv1 devices out of digital app assignment.
- Prefer DAXv2 when SmartSDR is v4.2.18 or newer and DAXv2 endpoints exist.
- Warn when both generations are present.
- If no DAX endpoints are visible, tell the user to start SmartSDR DAX and refresh.
