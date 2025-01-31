using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using Windows.ApplicationModel;

namespace WinUI3Template.Infrastructure.Helpers;

/// <summary>
/// Helper for startup register and unregister.
/// For MSIX package, you need to add extension: uap5:StartupTask.
/// Codes are edited from: <see href="https://github.com/microsoft/terminal"> and <see href="https://github.com/seerge/g-helper">.
/// </summary>
public class StartupHelper
{
    public const string NonMsixStartupTag = "/startup";

    private const string MsixTaskId = Constants.StartupTaskId;
    private const string NonMsixRegistryKey = Constants.StartupRegistryKey;
    private const string NonMsixLogonTaskName = Constants.StartupLogonTaskName;
    private const string NonMsixLogonTaskDesc = Constants.StartupLogonTaskDesc;

    private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string ApprovalPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    private static readonly byte[] ApprovalValue1 = [0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
    private static readonly byte[] ApprovalValue2 = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    /// <summary>
    /// Set application startup or not.
    /// </summary>
    public static async Task<bool> SetStartupAsync(bool startup, bool logon = false, bool currentUser = true)
    {
        if (RuntimeHelper.IsMSIX)
        {
            var startupTask = await StartupTask.GetAsync(MsixTaskId);
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
            var state = await GetStartupAsync(logon, currentUser);
            if (logon)
            {
                if (!state && startup)
                {
                    return ScheduleLogonTask();
                }
                else if (state && !startup)
                {
                    return UnscheduleLogonTask();
                }
            }
            else
            {
                if (!state && startup)
                {
                    return SetStartupRegistryKey(startup, currentUser);
                }
                else if (state && !startup)
                {
                    return SetStartupRegistryKey(startup, currentUser);
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Get application startup or not by checking register keys.
    /// </summary>
    public static async Task<bool> GetStartupAsync(bool logon = false, bool currentUser = true)
    {
        if (RuntimeHelper.IsMSIX)
        {
            var startupTask = await StartupTask.GetAsync(MsixTaskId);
            return startupTask.State == StartupTaskState.Enabled || startupTask.State == StartupTaskState.EnabledByPolicy;
        }
        else
        {
            if (logon)
            {
                using var taskService = new TaskService();
                return taskService.RootFolder.AllTasks.Any(t => t.Name == NonMsixLogonTaskName);
            }
            else
            {
                return CheckAndGetStartupRegistryKey(currentUser);
            }
        }
    }

    /// <summary>
    /// Check and fix the startup.
    /// </summary>
    public static async Task<bool> CheckStartup(bool currentUser = true)
    {
        if (RuntimeHelper.IsMSIX)
        {
            // Have checked but cannot do anything with MSIX package.
            return await GetStartupAsync(false, currentUser);
        }
        else
        {
            var check1 = CheckLogonTask();
            var check2 = CheckAndGetStartupRegistryKey(currentUser);
            return check1 || check2;
        }
    }

    /// <summary>
    /// Check the logon task.
    /// </summary>
    private static bool CheckLogonTask()
    {
        if (Environment.ProcessPath is not string appPath)
        {
            return false;
        }

        using var taskService = new TaskService();
        var task = taskService.RootFolder.AllTasks.FirstOrDefault(t => t.Name == NonMsixLogonTaskName);
        if (task != null)
        {
            try
            {
                var action = task.Definition.Actions.FirstOrDefault()!.ToString().Trim();
                if (!appPath.Equals(action, StringComparison.OrdinalIgnoreCase) && !File.Exists(action))
                {
                    UnscheduleLogonTask();
                    ScheduleLogonTask();
                }

                return true;
            }
            catch (Exception)
            {

            }
        }

        return false;
    }

    /// <summary>
    /// Schedule the logon task.
    /// </summary>
    private static bool ScheduleLogonTask()
    {
        if (Environment.ProcessPath is not string appPath)
        {
            return false;
        }

        using var td = TaskService.Instance.NewTask();
        td.RegistrationInfo.Description = NonMsixLogonTaskDesc;
        td.Triggers.Add(new LogonTrigger { UserId = WindowsIdentity.GetCurrent().Name, Delay = TimeSpan.FromSeconds(2) });
        td.Actions.Add(appPath, NonMsixStartupTag);

        if (RuntimeHelper.IsCurrentUserIsAdmin())
        {
            td.Principal.RunLevel = TaskRunLevel.Highest;
        }

        td.Settings.StopIfGoingOnBatteries = false;
        td.Settings.DisallowStartIfOnBatteries = false;
        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

        try
        {
            TaskService.Instance.RootFolder.RegisterTaskDefinition(NonMsixLogonTaskName, td);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Unschedule the logon task.
    /// </summary>
    private static bool UnscheduleLogonTask()
    {
        using var taskService = new TaskService();
        try
        {
            taskService.RootFolder.DeleteTask(NonMsixLogonTaskName);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Check and get the startup register key.
    /// </summary>
    private static bool CheckAndGetStartupRegistryKey(bool currentUser = true)
    {
        if (Environment.ProcessPath is not string appPath)
        {
            return false;
        }

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
                if (keyName.Equals(NonMsixRegistryKey, StringComparison.CurrentCultureIgnoreCase))
                {
                    startup = true;
                    // check if the startup register value is valid and fix it
                    if (startup)
                    {
                        var value = path.GetValue(keyName)!.ToString()!;
                        if (!value.Contains(@appPath, StringComparison.CurrentCultureIgnoreCase))
                        {
                            path.SetValue(NonMsixRegistryKey, $@"""{@appPath}"" {NonMsixStartupTag}");
                            path.Close();
                            path = root.OpenSubKey(ApprovalPath, true);
                            if (path != null)
                            {
                                path.SetValue(NonMsixRegistryKey, ApprovalValue1);
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
                        if (keyName.Equals(NonMsixRegistryKey, StringComparison.CurrentCultureIgnoreCase))
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

    /// <summary>
    /// Add or delete the startup register key.
    /// </summary>
    private static bool SetStartupRegistryKey(bool startup, bool currentUser = true)
    {
        if (Environment.ProcessPath is not string appPath)
        {
            return false;
        }

        var root = currentUser ? Registry.CurrentUser : Registry.LocalMachine;
        var value = $@"""{@appPath}"" {NonMsixStartupTag}";
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
                path.SetValue(NonMsixRegistryKey, value);
                path.Close();
                // set the startup approval key to approval status
                path = root.OpenSubKey(ApprovalPath, true);
                if (path != null)
                {
                    path.SetValue(NonMsixRegistryKey, ApprovalValue1);
                    path.Close();
                }
            }
            else
            // delete the startup register key
            {
                var keyNames = path.GetValueNames();
                foreach (var keyName in keyNames)
                {
                    if (keyName.Equals(NonMsixRegistryKey, StringComparison.CurrentCultureIgnoreCase))
                    {
                        path.DeleteValue(NonMsixRegistryKey);
                        path.Close();
                        break;
                    }
                }
                // delete the startup approval key
                path = root.OpenSubKey(ApprovalPath, true);
                if (path != null)
                {
                    path.DeleteValue(NonMsixRegistryKey);
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
