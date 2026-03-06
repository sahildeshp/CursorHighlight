# CursorHighlight

A lightweight open-source Windows utility that draws a semi-transparent circle around your cursor — useful when presenting your screen or recording tutorials.

![Windows](https://img.shields.io/badge/Windows-10%2F11-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

- Coloured highlight circle follows your cursor in real time (~60 fps)
- Lives in the system tray — no taskbar entry, no distraction
- Toggle the highlight on/off without quitting
- Configurable size, colour, and opacity with live preview
- Optional Windows startup launch (asks on first run)
- Single standalone `.exe` — no installer, no .NET runtime required on target machine

## Download

Grab the latest `CursorHighlight.exe` from the [Releases](../../releases) page and run it — no installation needed.

## Usage

1. Run `CursorHighlight.exe`
2. On first launch you'll be asked whether to start with Windows
3. A fluorescent yellow circle appears around your cursor immediately
4. Right-click the tray icon → **Settings** to adjust:
   - **Highlight enabled** — toggle the circle on/off
   - **Size** — circle diameter (20–200 px)
   - **Color** — click the colour swatch to pick any colour
   - **Opacity** — transparency of the circle (10–90%)
   - **Run at Windows startup** — enable/disable autostart
5. Right-click tray icon → **Exit** to quit

Settings are saved automatically to `%APPDATA%\CursorHighlight\settings.json`.

## Building from Source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Clone
git clone https://github.com/sahildeshp/CursorHighlight.git
cd CursorHighlight

# Run during development
dotnet run

# Build a standalone single .exe
dotnet publish -c Release -r win-x64 -o ./publish
```

The output `publish/CursorHighlight.exe` is fully self-contained (no .NET runtime needed on the target machine).

## Contributing

Contributions are welcome. Please open an issue before submitting a pull request for anything beyond small bug fixes.

1. Fork the repo
2. Create a feature branch (`git checkout -b feature/my-change`)
3. Commit your changes
4. Open a Pull Request

## License

MIT — see [LICENSE](LICENSE) for details.
