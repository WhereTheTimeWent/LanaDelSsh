using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LanaDelSsh.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LanaDelSsh.Views;

public partial class MainWindow : Window
{
    // Drag-and-drop tracking
    private int _dragFromIndex = -1;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SetupDragDrop();
        SetupPortInput();

    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await vm.InitializeAsync();
            var (w, h) = vm.Settings.GetSavedWindowSize();
            Width = w;
            Height = h;
        }
    }

    protected override void OnClosed(System.EventArgs e)
    {
        base.OnClosed(e);
        if (Vm?.Settings is not null)
            _ = Vm.Settings.SaveWindowSizeAsync(Width, Height);
    }

    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

    // ─── Port TextBox ────────────────────────────────────────────────────────

    private void SetupPortInput()
    {
        var tb = this.FindControl<TextBox>("PortTextBox");
        tb?.AddHandler(TextInputEvent, OnPortTextInput, RoutingStrategies.Tunnel);
    }

    private static void OnPortTextInput(object? sender, TextInputEventArgs e)
    {
        if (e.Text?.Any(c => !char.IsDigit(c)) == true)
            e.Handled = true;
    }

    private bool _settingPortText;

    private void OnPortTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_settingPortText || sender is not TextBox tb) return;
        var text = tb.Text ?? "";
        if (int.TryParse(text, out int val) && val > 65535)
        {
            _settingPortText = true;
            tb.Text = "65535";
            tb.CaretIndex = 5;
            _settingPortText = false;
        }
    }

    // ─── Quick Connect ───────────────────────────────────────────────────────

    private async void OnQuickConnectClick(object? sender, RoutedEventArgs e)
    {
        if (Vm?.QuickConnect is not null)
            await ConnectSafeAsync(() => Vm.QuickConnect.ConnectAsync(), () => Vm.QuickConnect.LastLaunchedProcess);
    }

private async void OnQuickConnectKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Vm?.QuickConnect is not null)
            await ConnectSafeAsync(() => Vm.QuickConnect.ConnectAsync(), () => Vm.QuickConnect.LastLaunchedProcess);
    }

    // ─── Saved Connections ───────────────────────────────────────────────────

    private async void OnListBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Vm?.SavedConnections.SelectedConnection is not null)
            await ConnectSafeAsync(() => Vm.SavedConnections.ConnectCommand.ExecuteAsync(null), () => Vm.SavedConnections.LastLaunchedProcess);
        else if (e.Key == Key.Delete && Vm?.SavedConnections.SelectedConnection is not null)
            await OnDeleteAsync();
    }

    private async void OnListBoxDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (Vm?.SavedConnections.SelectedConnection is not null)
            await ConnectSafeAsync(() => Vm.SavedConnections.ConnectCommand.ExecuteAsync(null), () => Vm.SavedConnections.LastLaunchedProcess);
    }

    private async void OnAddClick(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SavedConnections is null) return;

        var editVm = new ConnectionEditViewModel();
        var dialog = new ConnectionEditDialog { DataContext = editVm };
        var result = await dialog.ShowDialog<bool>(this);

        if (result)
        {
            await Vm.SavedConnections.AddConnectionAsync(editVm.BuildModel());
            FocusListBox();
        }
    }

    private async void OnEditClick(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SavedConnections is null) return;
        var selected = Vm.SavedConnections.SelectedConnection;
        if (selected is null) return;

        var editVm = new ConnectionEditViewModel(selected.ToModel());
        var dialog = new ConnectionEditDialog { DataContext = editVm };
        var result = await dialog.ShowDialog<bool>(this);

        if (result)
        {
            await Vm.SavedConnections.UpdateConnectionAsync(selected, editVm.BuildModel());
            FocusListBox();
        }
    }

    private async void OnDuplicateClick(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SavedConnections is null) return;
        var selected = Vm.SavedConnections.SelectedConnection;
        if (selected is null) return;

        var copy = selected.ToModel().Clone();
        copy.Name = copy.Name + " (Copy)";

        var editVm = new ConnectionEditViewModel(copy);
        var dialog = new ConnectionEditDialog { DataContext = editVm };
        var result = await dialog.ShowDialog<bool>(this);

        if (result)
        {
            await Vm.SavedConnections.AddConnectionAsync(editVm.BuildModel());
            FocusListBox();
        }
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e) => await OnDeleteAsync();

    private async Task OnDeleteAsync()
    {
        if (Vm?.SavedConnections is null) return;
        var selected = Vm.SavedConnections.SelectedConnection;
        if (selected is null) return;

        var loc = Localization.Loc.Instance;
        var msg = string.Format(loc.Confirm_Delete_Message, selected.Name);
        var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = loc.Confirm_Delete_Title,
            ContentMessage = msg,
            Icon = MsBox.Avalonia.Enums.Icon.Warning,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ButtonDefinitions =
            [
                new ButtonDefinition { Name = loc.Confirm_Yes, IsDefault = true },
                new ButtonDefinition { Name = loc.Confirm_No },
            ]
        });
        var result = await box.ShowWindowDialogAsync(this);

        if (result == loc.Confirm_Yes)
        {
            await Vm.SavedConnections.DeleteCommand.ExecuteAsync(null);
            FocusListBox();
        }
    }

    private async void OnConnectClick(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SavedConnections.SelectedConnection is null) return;
        await ConnectSafeAsync(() => Vm.SavedConnections.ConnectCommand.ExecuteAsync(null), () => Vm.SavedConnections.LastLaunchedProcess);
        FocusListBox();
    }

    private async void OnConnectKeepOpenClick(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SavedConnections.SelectedConnection is null) return;
        try
        {
            await Vm.SavedConnections.ConnectKeepOpenAsync();
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("No terminal emulator found"))
        {
            var loc = Localization.Loc.Instance;
            var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = loc.Error_NoTerminal_Title,
                ContentMessage = loc.Error_NoTerminal_Message,
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ButtonDefinitions = [new ButtonDefinition { Name = loc.Error_Ok, IsDefault = true }]
            });
            await box.ShowWindowDialogAsync(this);
        }
        FocusListBox();
    }

    private async Task ConnectSafeAsync(Func<Task> connect, Func<Process?>? getProcess = null)
    {
        try
        {
            await connect();
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("No terminal emulator found"))
        {
            var loc = Localization.Loc.Instance;
            var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = loc.Error_NoTerminal_Title,
                ContentMessage = loc.Error_NoTerminal_Message,
                Icon = MsBox.Avalonia.Enums.Icon.Error,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ButtonDefinitions = [new ButtonDefinition { Name = loc.Error_Ok, IsDefault = true }]
            });
            await box.ShowWindowDialogAsync(this);
            return;
        }

        if (getProcess != null)
            _ = MonitorProcessAsync(getProcess());
    }

    private async Task MonitorProcessAsync(Process? process)
    {
        if (process == null) return;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await process.WaitForExitAsync(cts.Token);
            // Exited within 2 seconds — show hint on UI thread
            await Dispatcher.UIThread.InvokeAsync(ShowQuickExitDialogAsync);
        }
        catch (OperationCanceledException) { /* Still running after 2s — normal */ }
        catch { /* Process not accessible */ }
    }

    private async Task ShowQuickExitDialogAsync()
    {
        var loc = Localization.Loc.Instance;
        var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = loc.Connect_QuickExitTitle,
            ContentMessage = loc.Connect_QuickExitMessage,
            Icon = MsBox.Avalonia.Enums.Icon.Info,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ButtonDefinitions = [new ButtonDefinition { Name = loc.Error_Ok, IsDefault = true }]
        });
        await box.ShowWindowDialogAsync(this);
    }

    private void OnPingClick(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SavedConnections.SelectedConnection is null) return;
        Vm.SavedConnections.PingCommand.Execute(null);
        FocusListBox();
    }

    private async void OnRemoveFromKnownHostsClick(object? sender, RoutedEventArgs e)
    {
        var vm = Vm?.SavedConnections;
        if (vm?.SelectedConnection is null) return;

        var host = vm.SelectedConnection.Host ?? string.Empty;
        var atIndex = host.IndexOf('@');
        var hostname = atIndex >= 0 ? host[(atIndex + 1)..] : host;

        var loc = Localization.Loc.Instance;
        string title, message;
        MsBox.Avalonia.Enums.Icon icon;

        try
        {
            bool removed = vm.RemoveSelectedFromKnownHosts();
            title = removed ? loc.KnownHosts_RemovedTitle : loc.KnownHosts_NotFoundTitle;
            message = string.Format(removed ? loc.KnownHosts_RemovedMessage : loc.KnownHosts_NotFoundMessage, hostname);
            icon = removed ? MsBox.Avalonia.Enums.Icon.Success : MsBox.Avalonia.Enums.Icon.Info;
        }
        catch (Exception)
        {
            title = loc.KnownHosts_ErrorTitle;
            message = loc.KnownHosts_ErrorMessage;
            icon = MsBox.Avalonia.Enums.Icon.Error;
        }

        var box = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = title,
            ContentMessage = message,
            Icon = icon,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ButtonDefinitions = [new ButtonDefinition { Name = loc.Error_Ok, IsDefault = true }]
        });
        await box.ShowWindowDialogAsync(this);
        FocusListBox();
    }

    private void FocusListBox()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var idx = ConnectionListBox.SelectedIndex;
            var item = idx >= 0 ? ConnectionListBox.ContainerFromIndex(idx) as ListBoxItem : null;
            (item ?? (Control)ConnectionListBox).Focus();
        }, Avalonia.Threading.DispatcherPriority.Background);
    }

    // ─── Settings / Close ────────────────────────────────────────────────────

    private void OnCloseWindowClick(object? sender, RoutedEventArgs e) => Close();

    private async void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        if (Vm?.Settings is null) return;
        Vm.Settings.ResetInvalidFolderInput();
        var dialog = new SettingsDialog { DataContext = Vm.Settings };
        await dialog.ShowDialog(this);
    }

    // ─── Drag and Drop ───────────────────────────────────────────────────────

    private void SetupDragDrop()
    {
        var listBox = this.FindControl<ListBox>("ConnectionListBox");
        if (listBox is null) return;

        listBox.AddHandler(PointerPressedEvent,  OnListPointerPressed,   handledEventsToo: true);
        listBox.AddHandler(PointerReleasedEvent, OnListPointerReleased,  handledEventsToo: true);
        listBox.AddHandler(PointerMovedEvent,    OnListPointerMoved,     handledEventsToo: true);
        listBox.AddHandler(DragDrop.DropEvent,   OnListDrop,             handledEventsToo: true);
        listBox.AddHandler(DragDrop.DragOverEvent, OnListDragOver,       handledEventsToo: true);
    }

    private bool _isDragging;
    private Avalonia.Point _dragStartPoint;
    private long _dragPressTimestamp;

    // Minimum time the pointer must be held before movement counts as a drag (ms).
    private const int DragMinHoldMs = 150;
    // Minimum Euclidean distance (px) the pointer must move to start a drag.
    private const double DragMinDistance = 10.0;

    private void OnListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _dragStartPoint = e.GetPosition(sender as Control);
        _dragPressTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        _isDragging = false;

        var listBox = sender as ListBox;
        if (listBox is null) return;
        _dragFromIndex = GetItemIndex(listBox, e);
    }

    private void OnListPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragFromIndex = -1;
        _isDragging = false;
    }

    private async void OnListPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragFromIndex < 0 || _isDragging) return;

        // Require the pointer to have been held long enough to distinguish drag from a quick click
        var elapsedMs = (System.Diagnostics.Stopwatch.GetTimestamp() - _dragPressTimestamp)
                        * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
        if (elapsedMs < DragMinHoldMs) return;

        var pos = e.GetPosition(sender as Control);
        var delta = pos - _dragStartPoint;
        if (delta.X * delta.X + delta.Y * delta.Y < DragMinDistance * DragMinDistance) return;

        _isDragging = true;
        var format = DataFormat.CreateStringApplicationFormat("DragIndex");
        var data = new DataTransfer();
        data.Add(DataTransferItem.Create(format, _dragFromIndex.ToString()));

        await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Move);
    }

    private void OnListDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Move;
        e.Handled = true;
    }

    private async void OnListDrop(object? sender, DragEventArgs e)
    {
        if (Vm?.SavedConnections is null) return;
        if (!string.IsNullOrWhiteSpace(Vm.SavedConnections.SearchText)) return;
        var listBox = sender as ListBox;
        if (listBox is null) return;

        var toIndex = GetItemIndex(listBox, e);
        if (toIndex < 0 || _dragFromIndex < 0 || toIndex == _dragFromIndex) return;

        await Vm.SavedConnections.MoveItemAsync(_dragFromIndex, toIndex);
        _dragFromIndex = -1;
        _isDragging = false;
    }

    private static int GetItemIndex(ListBox listBox, PointerEventArgs e)
    {
        var pos = e.GetPosition(listBox);
        return GetItemIndexAtPoint(listBox, pos);
    }

    private static int GetItemIndex(ListBox listBox, DragEventArgs e)
    {
        var pos = e.GetPosition(listBox);
        return GetItemIndexAtPoint(listBox, pos);
    }

    private static int GetItemIndexAtPoint(ListBox listBox, Avalonia.Point pos)
    {
        for (int i = 0; i < listBox.ItemCount; i++)
        {
            var container = listBox.ContainerFromIndex(i);
            if (container is null) continue;
            var bounds = container.Bounds;
            if (bounds.Contains(pos))
                return i;
        }
        return -1;
    }
}