using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using LanaDelSsh.Services;
using LanaDelSsh.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System.IO;

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

        if (File.Exists(currentFilePath))
        {
            if (File.Exists(newFilePath))
            {
                // Destination already has a file — ask before overwriting
                var loc = LanaDelSsh.Localization.Loc.Instance;
                var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
                {
                    ContentTitle = loc.Settings_ConnectionsFile_ExistsTitle,
                    ContentMessage = loc.Settings_ConnectionsFile_ExistsMessage,
                    Icon = MsBox.Avalonia.Enums.Icon.Question,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ButtonDefinitions =
                    [
                        new ButtonDefinition { Name = loc.Confirm_Yes, IsDefault = true },
                        new ButtonDefinition { Name = loc.Confirm_No, IsCancel = true }
                    ]
                });
                var result = await box.ShowWindowDialogAsync(this);
                if (result != loc.Confirm_Yes) return;
            }

            File.Move(currentFilePath, newFilePath, overwrite: true);
        }

        vm.ConnectionsFolder = folder;
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
