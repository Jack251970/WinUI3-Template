﻿namespace WinUI3Template.Infrastructure;

public static class Constants
{
    #region Program

    public const string WinUI3Template = "WinUI3Template";

    #endregion

    #region Startup

    public const string StartupRegistryKey = "WinUI3 Template";

    public const string StartupTaskId = "StartAppOnLoginTask";

    #endregion
    
    #region Resources

    public const string DefaultResourceFileName = "Resources";

    public static readonly string AppIconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Icon.ico");

    #endregion

    #region Settings & Logs

#if DEBUG
    public const string ApplicationDataFolder = "ApplicationData(Debug)";
#else
    public const string ApplicationDataFolder = "ApplicationData";
#endif

    public const string SettingsFolder = "Settings";
    
    public const string SettingsFile = "Settings.json";

    public const string LogsFolder = "Logs";

    #endregion
}