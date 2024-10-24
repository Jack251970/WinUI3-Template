using System.Runtime.InteropServices;

namespace WinUI3Template.Core.Helpers;

/// <summary>
/// Helper for actions related to windows system.
/// </summary>
public partial class SystemHelper
{
    #region check window existence

    internal const int SW_HIDE = 0;
    internal const int SW_SHOW = 5;
    internal const int SW_RESTORE = 9;
    internal const int WM_SHOWWINDOW = 0x0018;
    internal const int SW_PARENTOPENING = 3;

    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    internal static partial IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    /// <summary>
    /// Check if window exists and show window.
    /// </summary>
    public static bool IsWindowExist(string? className, string? windowName, bool showWindow)
    {
        var handle = FindWindow(className, windowName);
        if (handle != IntPtr.Zero)
        {
            if (showWindow)
            {
                // show window
                ShowWindow(handle, SW_RESTORE);
                ShowWindow(handle, SW_SHOW);
                SendMessage(handle, WM_SHOWWINDOW, 0, SW_PARENTOPENING);
                // bring window to front
                SetForegroundWindow(handle);
            }
            return true;
        }
        return false;
    }

    #endregion
}
