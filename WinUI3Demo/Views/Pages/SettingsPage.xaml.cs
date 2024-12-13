﻿using Microsoft.UI.Xaml.Controls;

namespace WinUI3Demo.Views.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = DependencyExtensions.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
    }
}