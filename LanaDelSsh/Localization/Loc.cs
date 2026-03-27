using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace LanaDelSsh.Localization;

/// <summary>
/// Observable singleton that exposes localized strings for XAML binding.
/// Usage in AXAML: {Binding Source={x:Static loc:Loc.Instance}, Path=QuickConnect_Title}
/// </summary>
public partial class Loc : ObservableObject
{
    private static readonly Lazy<Loc> _instance = new(() => new Loc());
    public static Loc Instance => _instance.Value;

    private Loc()
    {
        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Notify all string properties changed
        OnPropertyChanged(string.Empty);
    }

    private static string S(string key) => LocalizationService.Instance.Get(key);

    // App
    public string AppTitle => S("AppTitle");
    public string Settings => S("Settings");

    // Quick Connect
    public string QuickConnect_Title => S("QuickConnect_Title");
    public string QuickConnect_HostPlaceholder => S("QuickConnect_HostPlaceholder");
    public string QuickConnect_Port => S("QuickConnect_Port");
    public string QuickConnect_Connect => S("QuickConnect_Connect");
    public string QuickConnect_ValidationError => S("QuickConnect_ValidationError");

    // Saved Connections
    public string SavedConnections_Title => S("SavedConnections_Title");
    public string SavedConnections_Add => S("SavedConnections_Add");
    public string SavedConnections_Edit => S("SavedConnections_Edit");
    public string SavedConnections_Delete => S("SavedConnections_Delete");
    public string SavedConnections_Duplicate => S("SavedConnections_Duplicate");
    public string SavedConnections_Connect => S("SavedConnections_Connect");
    public string SavedConnections_Ping => S("SavedConnections_Ping");
    public string SavedConnections_ConnectKeepOpen => S("SavedConnections_ConnectKeepOpen");
    public string SavedConnections_RemoveFromKnownHosts => S("SavedConnections_RemoveFromKnownHosts");
    public string Connect_QuickExitTitle => S("Connect_QuickExitTitle");
    public string Connect_QuickExitMessage => S("Connect_QuickExitMessage");
    public string KnownHosts_RemovedTitle => S("KnownHosts_RemovedTitle");
    public string KnownHosts_RemovedMessage => S("KnownHosts_RemovedMessage");
    public string KnownHosts_NotFoundTitle => S("KnownHosts_NotFoundTitle");
    public string KnownHosts_NotFoundMessage => S("KnownHosts_NotFoundMessage");
    public string KnownHosts_ErrorTitle => S("KnownHosts_ErrorTitle");
    public string KnownHosts_ErrorMessage => S("KnownHosts_ErrorMessage");
    public string SavedConnections_Search => S("SavedConnections_Search");
    public string SavedConnections_Empty => S("SavedConnections_Empty");

    // Connection Edit Dialog
    public string ConnectionEdit_Title_Add => S("ConnectionEdit_Title_Add");
    public string ConnectionEdit_Title_Edit => S("ConnectionEdit_Title_Edit");
    public string ConnectionEdit_Name => S("ConnectionEdit_Name");
    public string ConnectionEdit_Host => S("ConnectionEdit_Host");
    public string ConnectionEdit_Port => S("ConnectionEdit_Port");
    public string ConnectionEdit_Ok => S("ConnectionEdit_Ok");
    public string ConnectionEdit_Cancel => S("ConnectionEdit_Cancel");

    // Confirm Dialog
    public string Confirm_Delete_Title => S("Confirm_Delete_Title");
    public string Confirm_Delete_Message => S("Confirm_Delete_Message");
    public string Confirm_Yes => S("Confirm_Yes");
    public string Confirm_No => S("Confirm_No");

    // Settings Dialog
    public string Settings_Title => S("Settings_Title");
    public string Settings_IgnoreCertificates => S("Settings_IgnoreCertificates");
    public string Settings_AutoUpdate => S("Settings_AutoUpdate");
    public string Settings_Theme => S("Settings_Theme");
    public string Settings_Theme_Auto => S("Settings_Theme_Auto");
    public string Settings_Theme_Dark => S("Settings_Theme_Dark");
    public string Settings_Theme_Light => S("Settings_Theme_Light");
    public string Settings_Theme_OceanBlvd => S("Settings_Theme_OceanBlvd");
    public string Settings_Language => S("Settings_Language");
    public string Settings_Language_Auto => S("Settings_Language_Auto");
    public string Settings_Language_English => S("Settings_Language_English");
    public string Settings_Language_German => S("Settings_Language_German");
    public string Settings_Close => S("Settings_Close");
    public string Settings_Window => S("Settings_Window");
    public string Settings_ResetWindowSize => S("Settings_ResetWindowSize");
    public string Settings_LinuxTerminal => S("Settings_LinuxTerminal");
    public string Settings_LinuxTerminal_Placeholder => S("Settings_LinuxTerminal_Placeholder");
    public string Settings_ConnectionsFile => S("Settings_ConnectionsFile");
    public string Settings_ConnectionsFile_Description => S("Settings_ConnectionsFile_Description");
    public string Settings_ConnectionsFile_Placeholder => S("Settings_ConnectionsFile_Placeholder");
    public string Settings_ConnectionsFile_ConflictTitle => S("Settings_ConnectionsFile_ConflictTitle");
    public string Settings_ConnectionsFile_ConflictMessage => S("Settings_ConnectionsFile_ConflictMessage");
    public string Settings_ConnectionsFile_KeepLocal => S("Settings_ConnectionsFile_KeepLocal");
    public string Settings_ConnectionsFile_KeepExisting => S("Settings_ConnectionsFile_KeepExisting");

    // Error dialogs
    public string Error_NoTerminal_Title => S("Error_NoTerminal_Title");
    public string Error_NoTerminal_Message => S("Error_NoTerminal_Message");
    public string Error_Ok => S("Error_Ok");
}
