# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**LanaDelSsh** is a cross-platform SSH connection manager built with C# and Avalonia UI (.NET 10). It provides a GUI for managing and launching SSH connections via the system's native terminal.

## Build & Run Commands

```bash
# Build
dotnet build

# Run
dotnet run --project LanaDelSsh/LanaDelSsh.csproj

# Publish (Windows x64)
dotnet publish -c Release -r win-x64

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~ConnectionEditViewModelTests"

# Run a single test method
dotnet test --filter "FullyQualifiedName~ConnectionEditViewModelTests.IsValid_TrueWhenNameAndHostWithAt"
```

## Architecture

The project follows **MVVM** with manual dependency injection composed in `App.axaml.cs`.

### Service Composition Flow

`App.axaml.cs` manually constructs all services and ViewModels, then sets `MainWindow.DataContext = new MainWindowViewModel(...)`. `MainWindowViewModel` holds child VMs: `QuickConnectViewModel`, `SavedConnectionsViewModel`, `SettingsViewModel`.

`MainWindowViewModel.InitializeAsync()` is called from `MainWindow.axaml.cs` on `Loaded` — it loads settings, applies language, then loads child VMs.

### Key Structural Patterns

- **ViewModels** use `CommunityToolkit.Mvvm` source generation: `[ObservableProperty]`, `[RelayCommand]`
- **Views** use Avalonia compiled bindings — enabled globally via `<AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>` in the `.csproj`; always set `x:DataType` on root elements
- **Services** are interface-backed and injected via constructor into ViewModels
- **JSON persistence** uses source-generated `AppJsonContext` (AOT-friendly) — stored in `%APPDATA%/LanaDelSsh/`; adding a new serialized type requires registering it in `AppJsonContext.cs`
- **Single-instance enforcement** via a named mutex in `Program.cs`

### Localization

`Loc` is an observable singleton whose properties call `LocalizationService.Instance.Get(key)`. When the language changes, `OnPropertyChanged(string.Empty)` is fired, refreshing all AXAML bindings like `{Binding Source={x:Static loc:Loc.Instance}, Path=SomeKey}`.

**To add a new UI string:**
1. Add a `<data>` entry to `Resources/Strings.resx` (English)
2. Add the same key to `Resources/Strings.de.resx` (German)
3. Add a property to `Localization/Loc.cs` calling `S("YourKey")`

### Theme & Design System

`AppSettings.ThemeVariant` stores: `null` (auto), `"light"`, `"dark"`, or a custom design key (e.g. `"ocean-blvd"`).

`DesignService` (`Services/DesignService.cs`) applies custom visual designs by merging a `ResourceDictionary` into `Application.Resources.MergedDictionaries` at runtime. Designs override Avalonia FluentTheme color keys and define app-specific resource keys (e.g. `AppWindowBackground`).

**Critical: FluentTheme bakes many brushes via `StaticResource`** — overriding the underlying `Color` key alone has no effect for those controls. The brush resource itself must be overridden directly. Known affected resources (always override as `SolidColorBrush`, not `Color`): `SystemControlBackgroundChromeMediumLowBrush`, `ComboBoxDropDownBackground`, `ButtonBackground*`, `AccentButtonBackground*`, `AccentButtonForeground*`, `AccentButtonBorderBrush*`, `MenuFlyoutPresenterBackground`, `MenuFlyoutItemBackground*`, `TextControlBackgroundFocused`. When adding new design overrides, fetch the relevant Fluent theme XAML from GitHub (`https://raw.githubusercontent.com/AvaloniaUI/Avalonia/master/src/Avalonia.Themes.Fluent/Controls/<Control>.xaml`) to find the exact resource key names.

**To add a new design:**
1. Add a `public const string` key in `DesignService`
2. Add a `CreateXxxDictionary()` method returning a populated `ResourceDictionary`
3. Add a case in `CreateDesignDictionary()`
4. If the design forces a `ThemeVariant`, add a case in `ForcesThemeVariant()`
5. Add a `ComboBoxItem` in `SettingsDialog.axaml` and strings in both `.resx` files and `Loc.cs`
6. Map the new index in `SettingsViewModel` (`LoadAsync`, `SaveAsync`, `ApplyTheme`)

### Dialog Pattern

Dialogs (`ConnectionEditDialog`, `SettingsDialog`) are opened from `MainWindow.axaml.cs` code-behind using `ShowDialog(this)`, not from the ViewModel. The ViewModel exposes state; the view drives the dialog lifecycle.

### Cross-Platform SSH/Ping Launching

`SshLaunchService` and `PingService` detect the OS via `RuntimeInformation.IsOSPlatform()`:
- **Windows**: launches via `cmd.exe`
- **macOS**: writes a temp shell script, opens with `Terminal.app`
- **Linux**: tries `x-terminal-emulator`, `gnome-terminal`, `xfce4-terminal`, `konsole`, `xterm` in order

## Key Dependencies

| Package | Purpose |
|---|---|
| `Avalonia 11.3.12` + `Avalonia.Themes.Fluent` | UI framework with Fluent theme |
| `CommunityToolkit.Mvvm 8.4.1` | MVVM source generation |
| `MessageBox.Avalonia 3.3.1.1` | Cross-platform message boxes |

## Code Conventions

- All code — variable names, method names, comments — must be written in **English**
- The application UI is available in English and German (see `Resources/Strings.resx` and `Resources/Strings.de.resx`)

## Avalonia-Specific Notes

- Files use `.axaml` extension; namespace is `https://github.com/avaloniaui`
- Styling uses CSS-like selectors (`Style Selector="Button.primary"`) — no WPF `Style.Triggers` or `TargetType`
- Use `IsVisible` (bool) not `Visibility` enum; `Opacity="0"` for invisible-but-space-occupying
- Shadows via `BoxShadow` on `Border`, not `DropShadowEffect`
- Asset URIs use `avares://LanaDelSsh/Assets/filename`, not `pack://`
- Use `DynamicResource` for theme-aware or runtime-swappable resources, `StaticResource` for fixed values
- **`DynamicResource` removal does not reliably revert inline-bound properties to the theme default at runtime.** When a resource key is removed from `MergedDictionaries`, Avalonia sets any inline-bound property to `null` (not `UnsetValue`), leaving windows with a transparent background that renders white. The fix used here: after removing a design's `ResourceDictionary`, call `window.ClearValue(TemplatedControl.BackgroundProperty)` on every open window — `ClearValue` truly unsets the local value and lets FluentTheme's `ControlTheme` styling take over. At startup this issue does not appear because the resource was never found, so the property was never set in the first place. See `DesignService.ClearWindowBackgrounds`.
- Do **not** use a global `Style Selector="Window"` setter for `AppWindowBackground` — an unresolved `DynamicResource` in a style setter strips the theme background the same way. Always bind inline on each `Window`.

## MCP Server

`.mcp.json` configures an Avalonia documentation MCP server — use it when looking up Avalonia APIs or migrating patterns.
