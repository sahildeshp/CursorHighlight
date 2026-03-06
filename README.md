# CursorHighlight

A lightweight Windows utility that draws a semi-transparent circle around your cursor — useful when presenting your screen.

## Features

- Coloured highlight circle follows your cursor in real time
- Lives in the system tray, no taskbar entry
- Toggle the highlight on/off from the Settings panel
- Configurable size, colour, and opacity
- Optional Windows startup launch
- Single standalone `.exe` — no installer or .NET runtime required on the target machine

## Usage

1. Run `CursorHighlight.exe`
2. On first launch, you'll be asked whether to start with Windows
3. The yellow circle appears around your cursor immediately
4. Right-click the tray icon → **Settings** to adjust:
   - **Highlight enabled** — toggle the circle on/off
   - **Size** — circle diameter (20–200 px)
   - **Color** — click the colour swatch to pick any colour
   - **Opacity** — how transparent the circle is (10–90%)
   - **Run at Windows startup** — enable/disable autostart
5. Right-click tray icon → **Exit** to quit

## Building

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Run during development
dotnet run

# Build a standalone single .exe
dotnet publish -c Release -r win-x64 -o ./publish
```

The output `publish/CursorHighlight.exe` is fully self-contained.

## Settings storage

Settings are saved to `%APPDATA%\CursorHighlight\settings.json`.
