using System.Collections.Specialized;
using System.Web;

using Microsoft.Windows.AppNotifications;

namespace WinUI3Template.Services;

internal class AppNotificationService(INavigationService navigationService) : IAppNotificationService
{
    private readonly INavigationService _navigationService = navigationService;

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

        AppNotificationManager.Default.Register();
    }

    // Handle notification invocations when your app is already running based on the notification arguments.
    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        /*switch (ParseArguments(args.Argument)["action"])
        {
            case "Dashboard":
                App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                {
                    _navigationService.NavigateTo(typeof(DashboardPageViewModel).FullName!);
                    App.MainWindow.Show();
                });
                break;
        }*/
    }

    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public void TryShow(string payload)
    {
        try
        {
            Show(payload);
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    public void RunShow(string payload)
    {
        Task.Run(() => TryShow(payload));
    }

    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
