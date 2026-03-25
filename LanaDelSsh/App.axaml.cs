using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using LanaDelSsh.Models;
using LanaDelSsh.Services;
using LanaDelSsh.ViewModels;
using LanaDelSsh.Views;
using System.Linq;

namespace LanaDelSsh;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            // Compose services
            var settingsService = new SettingsService();
            var connectionStorageService = new ConnectionStorageService();
            var sshLaunchService = new SshLaunchService();
            var pingService = new PingService();
            var knownHostsService = new KnownHostsService();

            var settings = settingsService.LoadAsync().GetAwaiter().GetResult();
            ApplyTheme(settings);

            // Apply custom connections folder before ViewModels subscribe to FileChanged
            connectionStorageService.SetFolder(settings.ConnectionsFolder);

            // Compose view models
            var quickConnectVm = new QuickConnectViewModel(sshLaunchService, settingsService);
            var savedConnectionsVm = new SavedConnectionsViewModel(
                connectionStorageService, sshLaunchService, pingService, settingsService, knownHostsService);
            var settingsVm = new SettingsViewModel(settingsService, connectionStorageService);

            var mainVm = new MainWindowViewModel(
                quickConnectVm, savedConnectionsVm, settingsVm, settingsService);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm,
                Width = settings.WindowWidth,
                Height = settings.WindowHeight
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ApplyTheme(AppSettings settings)
    {
        if (settings.ThemeVariant == DesignService.OceanBlvd)
        {
            DesignService.Apply(DesignService.OceanBlvd, this);
        }
        else
        {
            DesignService.Apply(null, this);
            RequestedThemeVariant = settings.ThemeVariant switch
            {
                "light" => ThemeVariant.Light,
                "dark"  => ThemeVariant.Dark,
                _       => ThemeVariant.Default
            };
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        foreach (var plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}