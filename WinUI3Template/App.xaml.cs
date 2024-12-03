﻿using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace WinUI3Template;

public partial class App : Application
{
    private static string ClassName => typeof(App).Name;

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

#if !DISABLE_XAML_GENERATED_MAIN
    private static bool IsExistWindow { get; set; } = false;
#endif

    public static bool CanCloseWindow { get; set; } = false;

    #endregion

    #region Constructor

    public App()
    {
#if !DISABLE_XAML_GENERATED_MAIN
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

        // Initialize core helpers before services
        LocalSettingsHelper.Initialize();

        // Build the host
        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
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
                services.AddTransient<NavShellViewModel>();
                services.AddTransient<NavShellPage>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<HomePage>();
                services.AddTransient<SettingsViewModel>();
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
    }

#endregion

    #region App Lifecycle

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
#if !DISABLE_XAML_GENERATED_MAIN
        if (IsExistWindow)
        {
            return;
        }
#endif

        base.OnLaunched(args);

        // Ensure the current window is active
        if (MainWindow != null)
        {
            return;
        }

        Debug.WriteLine($"App launched. Launch args type: {args.GetType().Name}.");

        // Create main window
        MainWindow = new MainWindow();
        await GetService<IActivationService>().ActivateMainWindowAsync(args);

        // Initialize dialog service
        GetService<IDialogService>().Initialize();
    }

    private static void HandleAppUnhandledException(Exception? ex, bool showToastNotification)
    {
        var exceptionString = ExceptionFormatter.FormatExcpetion(ex);

        Debugger.Break();

        Debug.WriteLine($"An unhandled error occurred : {exceptionString}");

        if (showToastNotification)
        {
            GetService<IAppNotificationService>().TryShow(
                string.Format("AppNotificationUnhandledExceptionPayload".GetLocalizedString(),
                $"{ex?.ToString()}{Environment.NewLine}"));
        }
    }

    public async Task OnActivatedAsync(AppActivationArguments activatedEventArgs)
    {
        var activatedEventArgsData = activatedEventArgs.Data;

        Debug.WriteLine($"The app is being activated. Activation type: {activatedEventArgsData.GetType().Name}");

        // InitializeApplication accesses UI, needs to be called on UI thread
        await MainWindow.EnqueueOrInvokeAsync((window) => window.InitializeApplicationAsync(activatedEventArgsData));
    }

    public static async new void Exit()
    {
        Debug.WriteLine("Exit current application.");

        // Unregister app notification service
        GetService<IAppNotificationService>().Unregister();

        // Close all windows
        await WindowsExtensions.CloseAllWindowsAsync();

        Current.Exit();
    }

    public static void RestartApplication()
    {
        // Get the path to the executable
        var exePath = Process.GetCurrentProcess().MainModule?.FileName;

        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
        {
            // Start a new instance of the application
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            });

            // exit the current application
            CanCloseWindow = true;
            MainWindow.Close();
        }
    }

    #endregion
}
