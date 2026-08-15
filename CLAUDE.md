# D4Companion - Claude Code guidance

Diablo 4 loot/overlay helper (WPF desktop app). This repo is a fork: `upstream` is
`josdemmers/Diablo4Companion`, `origin` is your fork. Keep `master` synced with upstream;
do feature work on `feat/*` branches.

## Platform - Windows only

- Every project targets `net10.0-windows` (WPF). Builds and runs on Windows ONLY, never from WSL/Linux.
- Build: `dotnet build -c Release` (or Visual Studio). Output: `D4Companion/bin/Release/net10.0-windows/D4Companion.exe`.
- Test: `dotnet test` (NUnit). Run from a Windows shell.

### C# tooling / navigation

- Full C# intelligence (LSP, build, refactor) needs the **Windows .NET SDK**. A Roslyn/OmniSharp LSP run from WSL/Linux cannot load the WPF project - `net10.0-windows` requires the Windows Desktop SDK, which does not exist on Linux.
- On Windows: a Roslyn LSP + a C# best-practices agent are available - use them for analysis and refactoring.
- On WSL/Linux: no working C# LSP. Navigate with `ast-grep` (structural, e.g. `ast-grep -l cs -p '...'`) and `rg` (text); edit, then build/verify on the Windows side.

## Architecture

Detection pipeline: screen capture -> OCR -> affix matching -> overlay render.

- `D4Companion` - WPF app, MVVM (`ViewModels/`).
- `D4Companion.Services` - core logic: `ScreenCapture.cs`, `OcrHandler.cs` (TesseractOCR + Emgu.CV),
  `AffixManager.cs` (FuzzierSharp matching), build importers (`BuildsManagerMaxroll.cs`, `BuildsManagerD2Core.cs`).
- `D4Companion.Helpers` - pure utilities, e.g. `WeaponTypeResolver.cs`.
- `D4Companion.Messages` - pub/sub between view models and services.
- `.Interfaces` `.Entities` `.Constants` `.Extensions` `.Localization` `.Updater` - support projects.
- `D4Companion.Tests` - NUnit.

## Gotchas (hard constraints, not style)

- **Overlay self-capture:** `ScreenCapture.cs` grabs the game window with `SRCCOPY | CAPTUREBLT`.
  CAPTUREBLT includes layered windows, and the GameOverlay.Net surface IS one, so the app photographs
  its own overlay and feeds it back into detection. Any overlay element drawn near the tooltip corrupts
  affix-marker placement. Anchor new overlay elements to a WINDOW CORNER, never to `_currentTooltip.Location`.
- **Weapon subtype matching is symmetric on purpose:** `AffixManager.IsTypeMatch` + `WeaponTypeResolver`.
  The Arsenal damage-type suffix appears only on Barbarian tooltips in English data; everyone else resolves
  to plain `weapon`. Plain `weapon` <-> Arsenal subtype matching must stay bidirectional or non-Barbarian /
  non-English users lose weapon matching. Do not simplify it to one-directional.

## Test gotchas

- `FuzzierSharpTests.WeightedRatioScorerTest` has ~10 pre-existing upstream failures. Not local work, do not chase them.
- `ScreenshotReplayTests.cs` currently matches zero affixes on saved captures; not usable for offline detection checks yet.
