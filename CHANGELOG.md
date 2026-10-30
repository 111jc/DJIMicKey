# Changelog

## 1.0.0 — First public release

- First public stable release of the DJI Mic Key Windows utility.
- Includes configurable receiver-button shortcut mapping, microphone input metering, tray operation, and diagnostics.
- Distributed as a versioned standalone executable and a portable ZIP.

## Pre-public development history

The entries below describe private development snapshots. Their version numbers are not public release tags; v1.0.0 is the first public release.

### Internal development 1.9.2

- Remove native frame extension and keep caption styles stripped during modal enable/restore cycles.
- Replace directional system shadows with a symmetric, click-through alpha shadow cached by window size.
- Remove the main maximize button and reject maximization/title double-click in both main and diagnostic windows.
- Refresh the main content once after closing diagnostics; synchronize shadow bounds after tray/minimize restoration.
- Verify repeated modal cycles, rejected maximize attempts, minimize/restore, actual screen content and symmetric shadow pixels.

### Internal development 1.9.1

- Restore native DWM window shadows while retaining the custom title bar.
- Buffer container painting and remove recursive full-window redraws from layout and status updates.
- Flash the transmitter LED on source-key down, independently of mapping enablement; suppress repeats and stop the pulse timer after 300 ms or when hidden.
- Add interactive regression checks for DWM rendering, localized status painting and source-event LED feedback.

### Internal development 1.9.0

- Match the refined HTML layout, typography, transmitter illustration, scalable action icons and gradient keycaps.
- Enable scaling compatibility automatically and remove its separate switch.
- Add a white footer with distinct listener/mapping colors, trigger count and latest trigger time.
- Balance both columns and share keycap wrapping between measurement and rendering.
- Keep explicit Record / Cancel / Apply and save states; persistence failures retain the draft and previous mapping for retry.
- Use a light diagnostic console matching the reference and retain native, single-file deployment.

### Internal development 1.8.1

- Replace the native-checkbox/self-painted switch mix with a fully managed, opaque switch control.
- Remove transparent card/label repaint chains that left overlapping text and black rectangles in live windows.
- Verify repeated switch changes and window resizing with a capture of the actual on-screen window.

### Internal development 1.8.0

- Rebuild the main window from the supplied HTML: custom title bar, two-column cards, switches, transmitter illustration and keycaps.
- Add a real input-peak history display and keep microphone testing user controlled.
- Use a single Start recording / Save and finish action.
- Move live input diagnostics into a dedicated modal window with export and listener restart.

### Internal development 1.7.0

- Show live raw HID and media-key diagnostics in the main window; no separate save step is required to view events.
- Extend the bounded button-test window to 60 seconds so a transmitter button test can be run alongside the live log.

### Internal development 1.6.0

- Show captured key, multimedia-command and DJI HID events live in the diagnostic panel.
- Keep a bounded on-screen log and retain optional report export.
- Publish the v1.5 transmitter-power test showing no Windows-visible short-press event in the tested USB setup.

### Internal development 1.5.0

- Extend the opt-in Win11 diagnostic to log VK_POWER, WM_APPCOMMAND, and raw DJI HID input.
- Record the transmitter power-button comparison: its short press did not reach Windows in the tested connection mode. No unreliable remap was added.

### Internal development 1.4.0

- Add an opt-in bounded media-key/raw-HID diagnostic report and native hook health probe.
- Add listener restart and visible application version.
- Report a stopped listener thread and decouple HID mapping readiness from audio-endpoint readiness.
- Add regression checks for diagnostics and health probing. The reported Win11 issue still requires a real-device diagnostic capture; this release does not claim a confirmed Win11 fix.

### Internal development 1.3.0

- Add a title-bar close dialog with explicit Exit and Minimize to tray choices.
- Preserve mapping and stop audio metering in tray mode; allow cancelling the close action.
- Validate the actual modal choices, direct Quit and dialog scaling.

### Internal development 1.2.0

- Add a system-font compatibility mode with adaptive, scrollable layouts.
- Handle per-monitor DPI changes without cumulative scaling.
- Add optional per-user startup registration and a tray-only automatic-start mode.
- Remove the main-window Exit button; closing the window now exits the application.
- Add isolated startup-setting checks and layout/lifecycle validation.

### Internal development 1.1.0

- Fix DJI consumer-control events being ignored because Windows marks them as injected input.
- Replace shortcut dropdowns with direct key/chord recording, preserving modifier sides.
- Move the keyboard hook onto a dedicated native-message thread.
- Add regression checks for DJI-like input and shortcut recording.

### Internal development 1.0.0

- Detect the supported DJI receiver and active capture endpoint.
- Display live microphone input levels.
- Map the connection button's Volume Up event to a configurable keyboard shortcut.
- Save user settings and support tray operation.
