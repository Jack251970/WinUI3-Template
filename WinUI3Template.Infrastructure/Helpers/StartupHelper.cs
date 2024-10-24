using Microsoft.Win32;
using Windows.ApplicationModel;

namespace WinUI3Template.Infrastructure.Helpers;

/// <summary>
/// Helper for startup register and unregister.
/// For MSIX package, you need to add extension: uap5:StartupTask.
/// Codes are edited from: <see href="https://github.com/microsoft/terminal">.
/// </summary>
public class StartupHelper
{
    private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string ApprovalPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    private static readonly byte[] ApprovalValue1 = [0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
    private static readonly byte[] ApprovalValue2 = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    /// <summary>
    /// Set application startup or not.
    /// </summary>
    public static async Task<bool> SetStartupAsync(string taskId, string registryKey, bool startup, bool currentUser = true)
    {
        if (RuntimeHelper.IsMSIX)
        {
            var startupTask = await StartupTask.GetAsync(taskId);
            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    if (startup)
                    {
                        return await startupTask.RequestEnableAsync() == StartupTaskState.Enabled;
                    }
                    break;
                case StartupTaskState.DisabledByUser:
                    if (startup)
                    {
                        // TerminalTODO: GH#6254: define UX for other StartupTaskStates
                        // Reference: terminal_main\src\cascadia\TerminalSettingsEditor\AppLogic.cpp
                    }
                    break;
                case StartupTaskState.Enabled:
                    if (!startup)
                    {
                        startupTask.Disable();
                    }
                    break;
                case StartupTaskState.EnabledByPolicy:
                    if (!startup)
                    {
                        return false;
                    }
                    break;
                case StartupTaskState.DisabledByPolicy:
                    if (startup)
                    {
                        return false;
                    }
                    break;
            }
        }
        else
        {
            var state = await GetStartup(taskId, registryKey, currentUser);
            if (!state && startup)
            {
                return SetStartupRegistryKey(registryKey, startup, currentUser);
            }
            else if (state && !startup)
            {
                return SetStartupRegistryKey(registryKey, startup, currentUser);
            }
        }
        return true;
    }

    /// <summary>
    /// Get application startup or not by checking register keys.
    /// </summary>
    public static async Task<bool> GetStartup(string taskId, string registryKey, bool currentUser = true)
    {
        if (RuntimeHelper.IsMSIX)
        {
            var startupTask = await StartupTask.GetAsync(taskId);
            return startupTask.State == StartupTaskState.Enabled || startupTask.State == StartupTaskState.EnabledByPolicy;
        }
        else
        {
            var appPath = Environment.ProcessPath!;
            var root = currentUser ? Registry.CurrentUser : Registry.LocalMachine;
            try
            {
                var startup = false;
                var path = root.OpenSubKey(RegistryPath, true);
                if (path == null)
                {
                    var key2 = root.CreateSubKey("SOFTWARE");
                    var key3 = key2.CreateSubKey("Microsoft");
                    var key4 = key3.CreateSubKey("Windows");
                    var key5 = key4.CreateSubKey("CurrentVersion");
                    var key6 = key5.CreateSubKey("Run");
                    path = key6;
                }
                var keyNames = path.GetValueNames();
                // check if the startup register key exists
                foreach (var keyName in keyNames)
                {
                    if (keyName.Equals(registryKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        startup = true;
                        // check if the startup register value is valid
                        if (startup)
                        {
                            var value = path.GetValue(keyName)!.ToString()!;
                            if (!value.Contains(@appPath, StringComparison.CurrentCultureIgnoreCase))
                            {
                                startup = false;
                                path.DeleteValue(registryKey);
                                path.Close();
                                path = root.OpenSubKey(ApprovalPath, true);
                                if (path != null)
                                {
                                    path.DeleteValue(registryKey);
                                    path.Close();
                                }
                            }
                        }
                        break;
                    }
                }
                // check if the startup register key is approved
                if (startup)
                {
                    path?.Close();
                    path = root.OpenSubKey(ApprovalPath, false);
                    if (path != null)
                    {
                        keyNames = path.GetValueNames();
                        foreach (var keyName in keyNames)
                        {
                            if (keyName.Equals(registryKey, StringComparison.CurrentCultureIgnoreCase))
                            {
                                var value = (byte[])path.GetValue(keyName)!;
                                if (!(value.SequenceEqual(ApprovalValue1) || value.SequenceEqual(ApprovalValue2)))
                                {
                                    startup = false;
                                }
                                break;
                            }
                        }
                    }
                }
                path?.Close();
                return startup;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Add or delete the startup register key.
    /// </summary>
    private static bool SetStartupRegistryKey(string registryKey, bool startup, bool currentUser = true)
    {
        var appPath = Environment.ProcessPath!;
        var root = currentUser ? Registry.CurrentUser : Registry.LocalMachine;
        var value = $@"""{@appPath}"" /startup";
        try
        {
            var path = root.OpenSubKey(RegistryPath, true);
            if (path == null)
            {
                var key2 = root.CreateSubKey("SOFTWARE");
                var key3 = key2.CreateSubKey("Microsoft");
                var key4 = key3.CreateSubKey("Windows");
                var key5 = key4.CreateSubKey("CurrentVersion");
                var key6 = key5.CreateSubKey("Run");
                path = key6;
            }
            // add the startup register key
            if (startup)
            {
                path.SetValue(registryKey, value);
                path.Close();
                // set the startup approval key to approval status
                path = root.OpenSubKey(ApprovalPath, true);
                if (path != null)
                {
                    path.SetValue(registryKey, ApprovalValue1);
                    path.Close();
                }
            }
            else
            // delete the startup register key
            {
                var keyNames = path.GetValueNames();
                foreach (var keyName in keyNames)
                {
                    if (keyName.Equals(registryKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        path.DeleteValue(registryKey);
                        path.Close();
                        break;
                    }
                }
                // delete the startup approval key
                path = root.OpenSubKey(ApprovalPath, true);
                if (path != null)
                {
                    path.DeleteValue(registryKey);
                    path.Close();
                }
            }
            path?.Close();
        }
        catch
        {
            return false;
        }
        return true;
    }
}
