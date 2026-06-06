# CAT / HRD Protocol

The MVP exposes one local TCP CAT server per managed slice.

```text
Slice A -> localhost:5101
Slice B -> localhost:5102
Slice C -> localhost:5103
Slice D -> localhost:5104
```

## MVP Commands

| Command | Meaning |
| --- | --- |
| `FA;` | Get frequency |
| `FA00014074000;` | Set frequency to 14.074 MHz |
| `MD;` | Get mode |
| `MDDIGU;` | Set mode |
| `TX;` | Get PTT |
| `TX1;` | PTT on |
| `TX0;` | PTT off |

Unknown commands return `?;` and should be logged once logging is fully wired.
