﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace WinUI3Template.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableRecipient, INavigationAware
{
    #region view properties

    public ObservableCollection<AppLanguageItem> AppLanguages = AppLanguageHelper.SupportedLanguages;

    public Visibility NonlogonTaskCardVisibility = RuntimeHelper.IsMSIX ? Visibility.Visible : Visibility.Collapsed;
    public Visibility LogonTaskExpanderVisibility = RuntimeHelper.IsMSIX ? Visibility.Collapsed : Visibility.Visible;

    [ObservableProperty]
    private int _languageIndex;

    [ObservableProperty]
    private bool _showRestartTip;

    [ObservableProperty]
    private bool _runStartup;

    [ObservableProperty]
    private bool _logonTask;

    [ObservableProperty]
    private int _themeIndex;

    [ObservableProperty]
    private int _backdropTypeIndex;

    [ObservableProperty]
    private string _appDisplayName = ConstantHelper.AppDisplayName;

    [ObservableProperty]
    private string _version = $"v{InfoHelper.GetVersion()}";

    [ObservableProperty]
    private string _copyRight = $"{InfoHelper.GetCopyright()}";

    #endregion

    private readonly IAppSettingsService _appSettingsService;
    private readonly IBackdropSelectorService _backdropSelectorService;
    private readonly IThemeSelectorService _themeSelectorService;

    private bool _isInitialized;

    public SettingsPageViewModel(IAppSettingsService appSettingsService, IBackdropSelectorService backdropSelectorService, IThemeSelectorService themeSelectorService)
    {
        _appSettingsService = appSettingsService;
        _backdropSelectorService = backdropSelectorService;
        _themeSelectorService = themeSelectorService;

        InitializeSettings();
    }

    private void InitializeSettings()
    {
        ThemeIndex = (int)_themeSelectorService.Theme;
        BackdropTypeIndex = (int)_appSettingsService.BackdropType;

        _isInitialized = true;
    }

    #region INavigationAware

    public async void OnNavigatedTo(object parameter)
    {
        LanguageIndex = AppLanguageHelper.SupportedLanguages.IndexOf(AppLanguageHelper.PreferredLanguage);

        var logonTask = await StartupHelper.GetStartupAsync(logon: true);
        var startupEntry = await StartupHelper.GetStartupAsync();
        RunStartup = logonTask || startupEntry;
        LogonTask = logonTask;

        ShowRestartTip = false;
    }

    public void OnNavigatedFrom()
    {

    }

    #endregion

    #region Commands

#pragma warning disable CA1822 // Mark members as static
    [RelayCommand]
    private void RestartApplication()
    {
        App.RestartApplication();
    }
#pragma warning restore CA1822 // Mark members as static

    [RelayCommand]
    private void CancelRestart()
    {
        ShowRestartTip = false;
    }

    #endregion

    #region Property Events

    partial void OnLanguageIndexChanging(int value)
    {
        if (_isInitialized)
        {
            if (RuntimeHelper.IsMSIX)
            {
                // No need to store the preference in packaged app - it is already stored by the app
                AppLanguageHelper.TryChange(value);
            }
            else
            {
                // No need to set PrimaryLanguageOverride in unpackaged app - it will be set by the app in the next launch
                _appSettingsService.SaveLanguageInSettingsAsync(AppLanguageHelper.GetLanguageCode(value));
            }

            ShowRestartTip = true;
        }
    }

    partial void OnRunStartupChanged(bool value)
    {
        if (_isInitialized)
        {
            if (value)
            {
                _ = StartupHelper.SetStartupAsync(true, logon: LogonTask);
            }
            else
            {
                _ = StartupHelper.SetStartupAsync(false, logon: true);
                _ = StartupHelper.SetStartupAsync(false);
            }
        }
    }

    partial void OnLogonTaskChanged(bool value)
    {
        if (_isInitialized)
        {
            if (RunStartup)
            {
                _ = StartupHelper.SetStartupAsync(false, logon: !value);
                _ = StartupHelper.SetStartupAsync(true, logon: value);
            }
        }
    }

    partial void OnThemeIndexChanged(int value)
    {
        if (_isInitialized)
        {
            _themeSelectorService.SetThemeAsync((ElementTheme)value);
        }
    }

    partial void OnBackdropTypeIndexChanged(int value)
    {
        if (_isInitialized)
        {
            _backdropSelectorService.SetBackdropTypeAsync((BackdropType)value);
        }
    }

    #endregion
}
