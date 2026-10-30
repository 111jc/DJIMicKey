# Validation

Baseline: Windows desktop application version 1.9.2, checked on 2026-10-30.

## Reproduce

Run `Test.cmd` from an interactive Windows desktop. Run `Test.cmd --device` to additionally check a connected receiver. Reports are generated under `.build/reports/` and excluded from Git.

## Verified

- 57 core checks: native interop layouts, key/chord generation, source-event suppression, debounce and release handling, modifier sides, shortcut capture, settings persistence, and isolated startup-registration writes.
- Windows-injected media events matching the observed DJI flags (0x11/0x91) are consumed and generate one trigger on release. Self-test events are intercepted by a safety hook rather than passed to other applications.
- Shortcut recording receives actual Windows input events, preserves modifier sides, requires an explicit save, supports cancellation and does not leak the recorded chord to other applications.
- Automatic compatibility layouts at simulated 100%, 125%, 150%, 175% and 200% scaling; additional 35% font enlargement at narrow width; repeated DPI transitions and the WM_DPICHANGED handler.
- Actual close-dialog buttons exercise Exit, Tray and Cancel: Exit releases hooks/audio/tray, Tray retains the listener and stops audio capture, and Cancel retains the visible window. Explicit Quit and Quit while the dialog is open exit without a redundant prompt.
- Close-dialog layouts are verified at 100%, 150% and 200% scaling with regular and legacy dialog settings.
- Automatic-start mode hides to the tray without opening audio capture; tray restoration shows the window.
- Connected receiver HID detection, active capture endpoint, WASAPI shared capture and live peak readings.
- A user has confirmed microphone-button start/stop of WeType voice input without changing system volume on the tested computer.

- Opt-in input diagnostics, bounded capture, media-only filtering, native health probes, raw-observer disposal and listener restart.
- The diagnostic UI prints startup, hook-health and captured events live without recording text or audio.

## Limits

- Hardware validation covers the observed VID_2CA3/PID_4011 receiver and Wireless Mic Rx capture endpoint, not every DJI product or connection mode.
- Power-button comparison: two transmitter Power presses produced no Windows media key, WM_APPCOMMAND or DJI HID reports; the separate pairing-button baseline produced the expected Volume Up and HID press/release. Receiver/audio and hook health checks passed. See docs/POWER_BUTTON_FINDINGS.md.
- The source hook currently matches Volume Up globally while the supported receiver is ready. It does not identify a particular device per key event.
- Desktop automation checks require a logged-in interactive session. A headless CI service is not equivalent.
- DPI tests simulate different scales and messages; they do not replace checking physical mixed-DPI monitors.
- The startup entry and automatic-start behavior have been verified without restarting the computer.
- Process memory depends on Windows, fonts and injected input-method modules. Startup measurements are not a lifetime memory guarantee.
- Mobile capabilities are a research plan, not tested or implemented features.

- A user reports Win11 input interception failure with a working receiver/meter. The diagnostic release has not been validated on that Win11 machine and does not establish a unique cause or confirmed fix.

- v1.8 redesign: single-button draft/save uses an isolated temporary settings file during UI checks. Diagnostic modal closes its observer and retains the report; both normal and compatibility layouts were checked through the existing scale matrix.

- v1.8.1: repeated switch changes and window resizes are captured from the real on-screen window (not only DrawToBitmap) to detect repaint artifacts.

- v1.9: selected HTML typography with explicit Chinese fallback, rich status footer, shared keycap wrapping/drawing and automatic compatibility are checked at the scale matrix. Recording/save failure retains the draft and prior mapping, then an explicit retry persists the new chord. Actual on-screen rendering was inspected against the supplied reference.

- v1.9.1: native DWM rendering confirmed enabled; shadow and illuminated LED captured over a white background. Safety-consumed Windows media-key events verify one flash on down despite key repeat, including with mapping paused. The pulse expires, repeated presses extend it, and hiding stops its timer. Footer updates produce zero canvas/waveform paint events in the regression. Physical microphone-button feedback still requires user confirmation.

- v1.9.2: exact alpha symmetry is checked across all four shadow edges; center is fully transparent. Three diagnostic modal cycles attempt system maximize, title double-click and programmatic maximize, then minimize/restore the main window. All remain normal-sized; main native caption/maximize style bits stay cleared, client/window sizes agree, controls remain enabled/visible, restored shadow bounds align, and actual screen captures confirm the header/content return. This replaces the old maximized states, which the app now rejects.
