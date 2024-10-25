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
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            applicationDataPath = Path.Combine(localAppDataPath, Constants.LocalAppDataFolder, Constants.ApplicationDataFolder);
        }
    }
}
