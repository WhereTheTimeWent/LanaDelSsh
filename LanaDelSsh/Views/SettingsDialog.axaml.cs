using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using LanaDelSsh.Models;
using LanaDelSsh.Services;
using LanaDelSsh.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System.IO;
using System.Text.Json;

namespace LanaDelSsh.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void OnBrowseConnectionsFileClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null || DataContext is not SettingsViewModel vm) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;

        var folder = folders[0].Path.LocalPath;
        var currentFilePath = vm.CurrentConnectionsFilePath;
        var newFilePath = Path.Combine(folder, ConnectionStorageService.FileName);

        if (currentFilePath == newFilePath) return;

        bool localHasData = FileHasConnections(currentFilePath);
        bool destinationExists = File.Exists(newFilePath);

        if (destinationExists && localHasData)
        {
            // Both files have data — ask which to keep
            var loc = LanaDelSsh.Localization.Loc.Instance;
            var folderName = new System.IO.DirectoryInfo(folder).Name;
            var keepExistingLabel = string.Format(loc.Settings_ConnectionsFile_KeepExisting, folderName);
            var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = loc.Settings_ConnectionsFile_ConflictTitle,
                ContentMessage = loc.Settings_ConnectionsFile_ConflictMessage,
                Icon = MsBox.Avalonia.Enums.Icon.Question,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ButtonDefinitions =
                [
                    new ButtonDefinition { Name = loc.Settings_ConnectionsFile_KeepLocal, IsDefault = true },
                    new ButtonDefinition { Name = keepExistingLabel },
                    new ButtonDefinition { Name = loc.ConnectionEdit_Cancel, IsCancel = true }
                ]
            });
            var result = await box.ShowWindowDialogAsync(this);

            if (result == loc.ConnectionEdit_Cancel) return;
            if (result == loc.Settings_ConnectionsFile_KeepLocal)
                File.Move(currentFilePath, newFilePath, overwrite: true);
            // else keepExistingLabel: just switch the folder, don't move anything
        }
        else if (!destinationExists && localHasData)
        {
            // No conflict — move local file to new location
            Directory.CreateDirectory(folder);
            File.Move(currentFilePath, newFilePath);
        }
        // else: local is empty — just switch, nothing to move

        vm.ConnectionsFolder = folder;
    }

    private static bool FileHasConnections(string path)
    {
        if (!File.Exists(path)) return false;
        try
        {
            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize(json, AppJsonContext.Default.ListSshConnection);
            return list is { Count: > 0 };
        }
        catch
        {
            return false;
        }
    }

    private async void OnGitHubClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is not null)
            await topLevel.Launcher.LaunchUriAsync(new Uri("https://github.com/WhereTheTimeWent/LanaDelSsh"));
    }

    private void OnResetWindowSizeClick(object? sender, RoutedEventArgs e)
    {
        const double defaultWidth = 550;
        const double defaultHeight = 620;

        if (Owner is Window owner)
        {
            owner.Width = defaultWidth;
            owner.Height = defaultHeight;
        }

        if (DataContext is LanaDelSsh.ViewModels.SettingsViewModel vm)
            _ = vm.ResetWindowSizeAsync();
    }
}
