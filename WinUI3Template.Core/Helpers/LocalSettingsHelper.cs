using Windows.Storage;

namespace WinUI3Template.Core.Helpers;

/// <summary>
/// Helpers for local settings.
/// </summary>
public class LocalSettingsHelper
{
    private static string applicationDataPath = string.Empty;
    public static string ApplicationDataPath => applicationDataPath;

    public static void Initialize()
    {
        if (RuntimeHelper.IsMSIX)
        {
            applicationDataPath = ApplicationData.Current.LocalFolder.Path;
        }
        else
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            applicationDataPath = Path.Combine(appDataPath, Constants.WinUI3Template, Constants.ApplicationDataFolder);
        }
    }

    public static string LogDirectory
    {
        get
        {
            var logDirectory = Path.Combine(ApplicationDataPath, Constants.LogsFolder);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            return logDirectory;
        }
    }
}
