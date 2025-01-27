using Microsoft.UI.Xaml;
using Windows.Foundation;
using WinUIEx;

namespace WinUI3Template.Core.Models;

public class DisplayMonitor
{
    public string Name { get; set; } = string.Empty;

    public Rect RectMonitor { get; set; }

    public Rect RectWork { get; set; }

    public bool IsPrimary { get; set; } = false;

    public static List<DisplayMonitor> GetMonitorInfo()
    {
        var monitorInfos = MonitorInfo.GetDisplayMonitors();
        return monitorInfos.Select(x => new DisplayMonitor
        {
            Name = x.Name,
            RectMonitor = x.RectMonitor,
            RectWork = x.RectWork,
            IsPrimary = x.IsPrimary
        }).ToList();
    }

    public static DisplayMonitor GetMonitorInfo(Window? window)
    {
        if (window is not null)
        {
            var monitorInfo = MonitorInfo.GetNearestDisplayMonitor(window.GetWindowHandle());
            if (monitorInfo is not null)
            {
                return new()
                {
                    Name = monitorInfo.Name,
                    RectMonitor = monitorInfo.RectMonitor,
                    RectWork = monitorInfo.RectWork,
                    IsPrimary = monitorInfo.IsPrimary
                };
            }
        }
        return GetPrimaryMonitorInfo();
    }

    public static DisplayMonitor GetPrimaryMonitorInfo()
    {
        var primaryMonitorInfo = MonitorInfo.GetDisplayMonitors().FirstOrDefault(x => x.IsPrimary);
        return new()
        {
            Name = primaryMonitorInfo!.Name,
            RectMonitor = primaryMonitorInfo.RectMonitor,
            RectWork = primaryMonitorInfo.RectWork,
            IsPrimary = primaryMonitorInfo.IsPrimary
        };
    }
}
