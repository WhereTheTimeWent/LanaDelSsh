using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace LanaDelSsh.Localization;

/// <summary>
/// Manages the active UI culture and provides string lookup via ResourceManager.
/// </summary>
public class LocalizationService
{
    private static readonly Lazy<LocalizationService> _instance =
        new(() => new LocalizationService());

    public static LocalizationService Instance => _instance.Value;

    private readonly ResourceManager _resourceManager =
        new("LanaDelSsh.Resources.Strings", typeof(LocalizationService).Assembly);

    private CultureInfo _currentCulture;

    private LocalizationService()
    {
        // Apply system culture on startup if no override
        _currentCulture = Thread.CurrentThread.CurrentUICulture;
    }

    public event EventHandler? LanguageChanged;

    public CultureInfo CurrentCulture => _currentCulture;

    /// <summary>
    /// Applies a language by culture code ("en", "de") or auto-detects from system (null/"").
    /// </summary>
    public void ApplyLanguage(string? languageCode)
    {
        var culture = languageCode switch
        {
            "de" => new CultureInfo("de"),
            "en" => new CultureInfo("en"),
            _ => DetectSupportedCulture()
        };

        _currentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string Get(string key)
    {
        try
        {
            return _resourceManager.GetString(key, _currentCulture) ?? key;
        }
        catch
        {
            return key;
        }
    }

    /// <summary>
    /// Detects system culture and maps it to a supported culture (de or en as fallback).
    /// </summary>
    private static CultureInfo DetectSupportedCulture()
    {
        var systemCulture = CultureInfo.CurrentUICulture;
        if (systemCulture.TwoLetterISOLanguageName == "de")
            return new CultureInfo("de");
        return new CultureInfo("en");
    }
}
