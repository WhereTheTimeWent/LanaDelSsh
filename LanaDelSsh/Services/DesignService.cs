using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;

namespace LanaDelSsh.Services;

/// <summary>
/// Applies named visual designs by merging / removing resource dictionaries at runtime.
/// To add a new design: add a key constant, a <see cref="ResourceDictionary"/> factory method,
/// a case in <see cref="CreateDesignDictionary"/>, and optionally in <see cref="ForcesThemeVariant"/>.
/// </summary>
public static class DesignService
{
    public const string OceanBlvd = "ocean-blvd";

    private static ResourceDictionary? _active;

    public static void Apply(string? designVariant, Application app)
    {
        // Remove any previously applied custom design
        if (_active is not null && app.Resources is ResourceDictionary appRes)
        {
            appRes.MergedDictionaries.Remove(_active);
            _active = null;

            // DynamicResource removal sets inline-bound properties to null rather than
            // UnsetValue, leaving windows with a transparent background. Explicitly
            // clearing the value lets FluentTheme's ControlTheme styling take over again.
            ClearWindowBackgrounds(app);
        }

        var dict = CreateDesignDictionary(designVariant);
        if (dict is null) return;

        if (app.Resources is ResourceDictionary resources)
        {
            resources.MergedDictionaries.Add(dict);
            _active = dict;
        }

        // Designs that imply a specific theme variant override it here
        if (ForcesThemeVariant(designVariant, out var forced))
            app.RequestedThemeVariant = forced;
    }

    /// <summary>Returns true when the given design forces its own ThemeVariant.</summary>
    public static bool ForcesThemeVariant(string? designVariant, out ThemeVariant forced)
    {
        switch (designVariant)
        {
            case OceanBlvd:
                forced = ThemeVariant.Dark;
                return true;
            default:
                forced = ThemeVariant.Default;
                return false;
        }
    }

    private static void ClearWindowBackgrounds(Application app)
    {
        if (app.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        foreach (var window in desktop.Windows)
            window.ClearValue(TemplatedControl.BackgroundProperty);
    }

    private static ResourceDictionary? CreateDesignDictionary(string? designVariant) => designVariant switch
    {
        OceanBlvd => CreateOceanBlvdDictionary(),
        _         => null
    };

    /// <summary>
    /// Dark: Did you know that there's a tunnel under Ocean Blvd
    /// Palette extracted from the Lana Del Rey album cover:
    ///   - Deep dark navy backgrounds (near-black tones of the photograph)
    ///   - Warm gold accent (color of the album title typography)
    ///   - Desaturated blue-gray chrome and text tones
    /// </summary>
    private static ResourceDictionary CreateOceanBlvdDictionary()
    {
        var dict = new ResourceDictionary();

        // Accent: warm gold from the album title typography
        dict["SystemAccentColor"]       = Color.Parse("#C8A84B");
        dict["SystemAccentColorDark1"]  = Color.Parse("#AC8C32");
        dict["SystemAccentColorDark2"]  = Color.Parse("#90721E");
        dict["SystemAccentColorDark3"]  = Color.Parse("#74590E");
        dict["SystemAccentColorLight1"] = Color.Parse("#D8B85C");
        dict["SystemAccentColorLight2"] = Color.Parse("#E6CA76");
        dict["SystemAccentColorLight3"] = Color.Parse("#F2DC96");

        // Chrome: deep navy tones from the dark areas of the photograph
        dict["SystemChromeLowColor"]          = Color.Parse("#0D1219");
        dict["SystemChromeMediumColor"]       = Color.Parse("#131A26");
        dict["SystemChromeMediumLowColor"]    = Color.Parse("#1C2B3F");
        dict["SystemChromeHighColor"]         = Color.Parse("#3D4F68");
        dict["SystemChromeWhiteColor"]        = Color.Parse("#C9C5BD");
        dict["SystemChromeBlackHighColor"]    = Color.Parse("#FF000000");
        dict["SystemChromeBlackMediumHighColor"] = Color.Parse("#CC000000");
        dict["SystemChromeBlackMediumColor"]  = Color.Parse("#99000000");
        dict["SystemChromeBlackLowColor"]     = Color.Parse("#33000000");

        // Base: cool blue-grey text tones matching the album's desaturated film highlights
        dict["SystemBaseHighColor"]       = Color.Parse("#C4C8D1");
        dict["SystemBaseMediumHighColor"] = Color.Parse("#ABAEBB");
        dict["SystemBaseMediumColor"]     = Color.Parse("#8C97AA");
        dict["SystemBaseMediumLowColor"]  = Color.Parse("#668A95A8");
        dict["SystemBaseLowColor"]        = Color.Parse("#338A95A8");

        // Alt: content area backgrounds (dark navy)
        dict["SystemAltHighColor"]       = Color.Parse("#0D1219");
        dict["SystemAltMediumHighColor"] = Color.Parse("#CC0D1219");
        dict["SystemAltMediumColor"]     = Color.Parse("#990D1219");
        dict["SystemAltMediumLowColor"]  = Color.Parse("#660D1219");
        dict["SystemAltLowColor"]        = Color.Parse("#330D1219");

        // List highlight tones (subtle gold tint)
        dict["SystemListLowColor"]          = Color.Parse("#19C8A84B");
        dict["SystemListMediumColor"]       = Color.Parse("#33C8A84B");
        dict["SystemRevealListLowColor"]    = Color.Parse("#19C8A84B");
        dict["SystemRevealListMediumColor"] = Color.Parse("#33C8A84B");

        // Control surface backgrounds (FluentTheme bakes these as brush resources via StaticResource,
        // so overriding the underlying Color alone has no effect — the brush must be overridden directly)
        dict["SystemControlBackgroundChromeMediumLowBrush"] = new SolidColorBrush(Color.Parse("#131A26"));
        dict["ComboBoxDropDownBackground"]                  = new SolidColorBrush(Color.Parse("#131A26"));

        // Button backgrounds — all interactive states
        dict["ButtonBackground"]             = new SolidColorBrush(Color.Parse("#1C2B3F"));
        dict["ButtonBackgroundPointerOver"]  = new SolidColorBrush(Color.Parse("#243650"));
        dict["ButtonBackgroundPressed"]      = new SolidColorBrush(Color.Parse("#131E2E"));
        dict["ButtonBackgroundDisabled"]     = new SolidColorBrush(Color.Parse("#0F1825"));

        // Accent button disabled state — matches regular disabled button exactly
        dict["AccentButtonBackgroundDisabled"]  = new SolidColorBrush(Color.Parse("#0F1825"));
        dict["AccentButtonForegroundDisabled"]  = new SolidColorBrush(Color.Parse("#66FFFFFF"));
        dict["AccentButtonBorderBrushDisabled"] = new SolidColorBrush(Color.Parse("#0F1825"));

        // Context menu background
        dict["MenuFlyoutPresenterBackground"]        = new SolidColorBrush(Color.Parse("#131A26"));
        dict["MenuFlyoutItemBackground"]             = new SolidColorBrush(Color.Parse("#131A26"));
        dict["MenuFlyoutItemBackgroundPointerOver"]  = new SolidColorBrush(Color.Parse("#1C2B3F"));
        dict["MenuFlyoutItemBackgroundPressed"]      = new SolidColorBrush(Color.Parse("#243650"));

        // TextBox focused background — override the StaticResource-baked brush directly
        dict["TextControlBackgroundFocused"] = new SolidColorBrush(Color.Parse("#1C2B3F"));

        // App-level brushes consumed by MainWindow and other surfaces
        dict["AppWindowBackground"] = new SolidColorBrush(Color.Parse("#0D1219"));

        return dict;
    }
}
