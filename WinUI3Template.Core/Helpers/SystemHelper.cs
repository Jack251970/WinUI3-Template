using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WinUI3Template.Core.Helpers;

/// <summary>
/// Helper for actions related to windows system.
/// </summary>
public partial class SystemHelper
{
    #region check window existence

    /// <summary>
    /// Check if window exists and show window.
    /// </summary>
    public static bool IsWindowExist(string? className, string? windowName, bool showWindow)
    {
        var hwnd = PInvoke.FindWindow(className, windowName);
        if (hwnd != IntPtr.Zero)
        {
            if (showWindow)
            {
                // show window
                PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_RESTORE);
                PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOW);
                PInvoke.SendMessage(hwnd, PInvoke.WM_SHOWWINDOW, 0, new LPARAM((nint)SHOW_WINDOW_STATUS.SW_PARENTOPENING));

                // bring window to front
                PInvoke.SetForegroundWindow(hwnd);
            }
            return true;
        }
        return false;
    }

    #endregion
}
