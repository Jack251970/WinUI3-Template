using System.Diagnostics;
#if !DISABLE_XAML_GENERATED_MAIN
using Microsoft.Extensions.Configuration;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Serilog;

namespace WinUI3Template;

public partial class App : Application
{
    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(App));

    #region Host

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost? Host { get; private set; }

    public static T GetService<T>() where T : class
    {
        if ((Current as App)!.Host!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    #endregion

    #region Main Window

    public static MainWindow MainWindow { get; set; } = null!;

#if !DISABLE_XAML_GENERATED_MAIN && SINGLE_INSTANCE
    private static bool IsExistWindow { get; set; } = false;
#endif

#if TRAY_ICON
    public static bool CanCloseWindow { get; set; } = false;
#endif

    #endregion

    #region Tray Icon

#if TRAY_ICON
    public static TrayMenuControl TrayIcon { get; set; } = null!;
#endif

    #endregion

    #region Splash Screen

    public static TaskCompletionSource? SplashScreenLoadingTCS { get; private set; }

    #endregion

    #region Constructor

    public App()
    {
#if !DISABLE_XAML_GENERATED_MAIN && SINGLE_INSTANCE
        // Check if app is already running
        if (SystemHelper.IsWindowExist(null, ConstantHelper.AppDisplayName, true))
        {
            IsExistWindow = true;
            Current.Exit();
            return;
        }
#endif

        // Initialize the component
        InitializeComponent();

#if !DISABLE_XAML_GENERATED_MAIN
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
#endif

        // Build the host
        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureLogging(builder => builder
                .AddSerilog(dispose: true))
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateOnBuild = true;
            })
            .ConfigureServices((context, services) =>
            {
                #region Core Service

                // Default Activation Handler
                services.AddSingleton<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers
                services.AddSingleton<IActivationHandler, AppNotificationActivationHandler>();

                // Windows Activation
                services.AddSingleton<IActivationService, ActivationService>();

                // Notifications
                services.AddSingleton<IAppNotificationService, AppNotificationService>();

                // File Storage
                services.AddSingleton<IFileService, FileService>();

                // Theme Management
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();

                // Backdrop Management
                services.AddSingleton<IBackdropSelectorService, BackdropSelectorService>();

                // Dependency Injection
                services.AddSingleton<IDependencyService, DependencyService>();

                // Dialog Managment
                services.AddSingleton<IDialogService, DialogService>();

                // Main window: Allow access to the main window
                // from anywhere in the application.
                services.AddSingleton(_ => (Window)MainWindow);

                // DispatcherQueue: Allow access to the DispatcherQueue for
                // the main window for general purpose UI thread access.
                services.AddSingleton(_ => MainWindow.DispatcherQueue);

                #endregion

                #region Navigation Service

                // MainWindow Pages
                services.AddSingleton<IPageService, PageService>();

                // MainWindow Navigation View
                services.AddSingleton<INavigationViewService, NavigationViewService>();

                // MainWindow Navigation
                services.AddSingleton<INavigationService, NavigationService>();

                #endregion

                #region Settings Service

                // Local Storage
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();

                // Settings Management
                services.AddSingleton<IAppSettingsService, AppSettingsService>();

                #endregion

                #region Views & ViewModels

                // Main Window Pages
                services.AddTransient<NavShellPageViewModel>();
                services.AddTransient<NavShellPage>();
                services.AddTransient<HomePageViewModel>();
                services.AddTransient<HomePage>();
                services.AddTransient<SettingsPageViewModel>();
                services.AddTransient<SettingsPage>();

                #endregion

                #region Configurations

                // Local Stettings
                services.Configure<LocalSettingsKeys>(context.Configuration.GetSection(nameof(LocalSettingsKeys)));

                #endregion
            })
            .Build();

        // Configure exception handlers
        UnhandledException += (sender, e) => HandleAppUnhandledException(e.Exception, true);
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => HandleAppUnhandledException(e.ExceptionObject as Exception, false);
        TaskScheduler.UnobservedTaskException += (sender, e) => HandleAppUnhandledException(e.Exception, false);

        // Initialize core extensions after services
        DependencyExtensions.Initialize(GetService<IDependencyService>());

        // Initialize core services
        GetService<IAppSettingsService>().Initialize();
        GetService<IAppNotificationService>().Initialize();

        // Initialize core helpers after services
        AppLanguageHelper.Initialize();

        _log.Information($"App initialized. Language: {AppLanguageHelper.PreferredLanguage}.");
    }

#endregion

    #region App Lifecycle

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

#if !DISABLE_XAML_GENERATED_MAIN && SINGLE_INSTANCE
        if (IsExistWindow)
        {
            return;
        }
#endif

        // Ensure the current window is active
        if (MainWindow != null)
        {
            return;
        }

        _ = ActivateAsync();

        async Task ActivateAsync()
        {
            // Get AppActivationArguments
            var appActivationArguments = AppInstance.GetCurrent().GetActivatedEventArgs();

            // Initialize the window
            MainWindow = new MainWindow();

#if SPLASH_SCREEN
            // Show the splash screen
            SplashScreenLoadingTCS = new TaskCompletionSource();
            await GetService<IActivationService>().LaunchMainWindowAsync(appActivationArguments);

            // Activate the window
            MainWindow.Activate();
#endif

            _log.Information($"App launched. Launch args type: {args.GetType().Name}.");

#if SPLASH_SCREEN
            static async Task WithTimeoutAsync(Task task, TimeSpan timeout)
            {
                if (task == await Task.WhenAny(task, Task.Delay(timeout)))
                {
                    await task;
                }
            }

            // Wait for the UI to update
            await WithTimeoutAsync(SplashScreenLoadingTCS!.Task, TimeSpan.FromMilliseconds(500));
            SplashScreenLoadingTCS = null;
#endif

            // Initialize dialog service
            GetService<IDialogService>().Initialize();

            // Check startup
            _ = StartupHelper.CheckStartup();

            // TODO: Initialize others things

            await GetService<IActivationService>().ActivateMainWindowAsync(args);
        }
    }

    private static void HandleAppUnhandledException(Exception? ex, bool showToastNotification)
    {
        var exceptionString = ExceptionFormatter.FormatExcpetion(ex);

        Debugger.Break();

        // Log the error
        _log.Fatal(ex, $"An unhandled error occurred : {exceptionString}");

        // Close the log
        Log.CloseAndFlush();

        // Try to show a notification
        if (showToastNotification)
        {
            GetService<IAppNotificationService>().TryShow(
                string.Format("AppNotificationUnhandledExceptionPayload".GetLocalizedString(),
                $"{ex?.ToString()}{Environment.NewLine}"));
        }

        // We are very likely in a bad and unrecoverable state, so ensure Dev Home crashes w/ the exception info.
        Environment.FailFast(exceptionString, ex);
    }

#if DISABLE_XAML_GENERATED_MAIN
    public async Task OnActivatedAsync(AppActivationArguments activatedEventArgs)
    {
        _log.Information($"App is activated. Activation type: {activatedEventArgs.Data.GetType().Name}");

        await MainWindow.EnqueueOrInvokeAsync(async (_) => await GetService<IActivationService>().ActivateMainWindowAsync(activatedEventArgs));
    }
#endif

    public static async new void Exit()
    {
        _log.Information("Exiting current application");

        // Unregister app notification service
        GetService<IAppNotificationService>().Unregister();

        // Close all windows
        await WindowsExtensions.CloseAllWindowsAsync();

        Current.Exit();
    }

    public static void RestartApplication(string? param = null, bool admin = false)
    {
        _log.Information("Restarting current application with args: {param}, admin: {admin}", param, admin);

        // Get the path to the executable
        var exePath = Process.GetCurrentProcess().MainModule?.FileName;

        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
        {
            // Start a new instance of the application
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                Arguments = param,
                Verb = admin ? "runas" : string.Empty
            });

            // Close the log
            Log.CloseAndFlush();

            // Kill the current process
            Process.GetCurrentProcess().Kill();
        }
    }

#endregion
}
