using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using LanaDelSsh.ViewModels;
using System;

namespace LanaDelSsh.Views;

public partial class ConnectionEditDialog : Window
{
    public ConnectionEditDialog()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.FindDescendantOfType<TextBox>()?.Focus();
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
