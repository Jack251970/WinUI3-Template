using Microsoft.Extensions.Configuration;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using Windows.Win32;
using Windows.Win32.Security;
using Windows.Win32.System.Com;

namespace WinUI3Template;

#if DISABLE_XAML_GENERATED_MAIN

/// <summary>
/// Represents the base entry point of the app.
/// </summary>
/// <remarks>
/// Gets called at the first time when the app launched or activated.
/// </remarks>
public class Program
{
#if DEBUG
    private const string AppInstanceKey = $"{Constants.WinUI3Template}-DEBUG";
#else
    private const string AppInstanceKey = $"{Constants.WinUI3Template}-RELEASE";
#endif

    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(Program));

    /// <summary>
    /// Initializes the process; the entry point of the process.
    /// </summary>
    /// <remarks>
    /// <see cref="Main"/> cannot be declared to be async because this prevents Narrator from reading XAML elements in a WinUI app.
    /// </remarks>
    [STAThread]
    private static void Main(string[] args)
    {
        // Initialize core helpers
        LocalSettingsHelper.Initialize();

        // Set up Logging
        Environment.SetEnvironmentVariable("LOGGING_ROOT", Path.Combine(LocalSettingsHelper.LogDirectory, InfoHelper.GetVersion().ToString()));
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        _log.Information($"Program launched with args: {string.Join(' ', [.. args])}");

        WinRT.ComWrappersSupport.InitializeComWrappers();

        var instance = AppInstance.FindOrRegisterForKey(AppInstanceKey);
        if (instance.IsCurrent)
        {
            instance.Activated += OnActivated;
        }
#if SINGLE_INSTANCE
        else
        {
            // Redirect activation to the existing instance
            RedirectActivationTo(instance, AppInstance.GetCurrent().GetActivatedEventArgs());

            // Kill the current process
            Environment.Exit(0);

            return;
        }
#endif

        Application.Start((p) =>
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
            SynchronizationContext.SetSynchronizationContext(context);
            _ = new App();
        });

        _log.Information("Program terminated");
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Gets invoked when the application is activated.
    /// </summary>
    private static async void OnActivated(object? sender, AppActivationArguments args)
    {
        // WINUI3: Verify if needed or OnLaunched is called
        if (Application.Current is App currentApp)
        {
            await currentApp.OnActivatedAsync(args);
        }
    }

#if SINGLE_INSTANCE
    /// <summary>
    /// Redirects the activation to the main process.
    /// </summary>
    /// <remarks>
    /// Redirects on another thread and uses a non-blocking wait method to wait for the redirection to complete.
    /// </remarks>
    public static void RedirectActivationTo(AppInstance keyInstance, AppActivationArguments args)
    {
        // Create an event for activation synchronization
        var eventHandle = PInvoke.CreateEvent((SECURITY_ATTRIBUTES?)null, true, false, null!);

        // Redirect activation asynchronously
        Task.Run(() =>
        {
            keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
            PInvoke.SetEvent(eventHandle);
        });

        // Wait for the activation redirection to complete
        _ = PInvoke.CoWaitForMultipleObjects(
            (uint)CWMO_FLAGS.CWMO_DEFAULT,
            PInvoke.INFINITE,
            [new(eventHandle.DangerousGetHandle())],
            out var handleIndex);
    }
#endif

}

#endif
