namespace LanaDelSsh.Models;

public class AppSettings
{
    public bool IgnoreCertificates { get; set; } = true;

    public bool AutoUpdate { get; set; } = true;

    /// <summary>
    /// null = auto-detect from system, "en" = English, "de" = German
    /// </summary>
    public string? LanguageCode { get; set; } = null;

    /// <summary>null = Auto, "light" = Light, "dark" = Dark, "ocean-blvd" = custom design</summary>
    public string? ThemeVariant { get; set; } = null;

    public double WindowWidth { get; set; } = 550;
    public double WindowHeight { get; set; } = 620;

    /// <summary>Custom Linux terminal executable (e.g. "kitty"). When set, takes precedence over built-in candidates.</summary>
    public string? LinuxTerminal { get; set; } = null;

    /// <summary>Custom folder for the connections file. Null = use default %APPDATA%/LanaDelSsh/.</summary>
    public string? ConnectionsFolder { get; set; } = null;
}
