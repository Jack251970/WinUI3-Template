// Copyright (c) 2024 Jack251970
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;

namespace WinUI3Demo.Core.Extensions;

/// <summary>
/// Provides static extension for windows, support multi-thread windows.
/// </summary>
public static class WindowsExtensions
{
    private static readonly Dictionary<Window, WindowLifecycleHandler> WindowsAndLifecycle = [];

    public static object? CurrentParameter { get; private set; }

    public static List<Window> GetAllWindows()
    {
        return new List<Window>(WindowsAndLifecycle.Keys);
    }

    #region create & close

    public static T CreateWindow<T>(bool isNewThread = false, WindowLifecycleActions? lifecycleActions = null, object? parameter = null) where T : Window, new()
    {
        return CreateWindow(() => new T(), isNewThread, lifecycleActions, parameter);
    }

    public static T CreateWindow<T>(Func<T> func, bool isNewThread = false, WindowLifecycleActions? lifecycleActions = null, object? parameter = null) where T : Window
    {
        T window = null!;
        DispatcherExitDeferral? deferral = null;

        CurrentParameter = parameter;
        if (isNewThread)
        {
            deferral = new DispatcherExitDeferral();

            var signal = new ManualResetEvent(false);

            var thread = new Thread(() =>
            {
                // create a DispatcherQueue on this new thread
                var dq = DispatcherQueueController.CreateOnCurrentThread();

                // initialize xaml in it and ResourceManagerRequested event will be called
                WindowsXamlManager.InitializeForCurrentThread();

                // invoke action before window creation
                lifecycleActions?.Window_Creating?.Invoke();

                // create a new window
                var window = func();

                // register window in ui thread extension
                ThreadExtensions.RegisterWindow(window);

                // register window in ui element extension
                var lifecycleHandler = new WindowLifecycleHandler
                {
                    ExitDeferral = deferral,
                    LifecycleActions = lifecycleActions ?? new()
                };
                RegisterWindow(window, lifecycleHandler);

                // invoke action after window creation
                lifecycleActions?.Window_Created?.Invoke(window);

                // signal that window creation is complete
                signal.Set();

                // run message pump
                dq.DispatcherQueue.RunEventLoop(DispatcherRunOptions.None, deferral);

                // invoke action before window closing
                lifecycleActions?.Window_Closing?.Invoke(window);

                // close window
                window.Close();

                // invoke action after window closing
                lifecycleActions?.Window_Closed?.Invoke();

                // signal that window closing is complete
                lifecycleActions?.CompletionSource?.SetResult();
            })
            {
                // will be destroyed when main is closed
                IsBackground = true
            };

            thread.Start();

            // wait for the signal
            signal.WaitOne();
        }
        else
        {
            // invoke action before window creation
            lifecycleActions?.Window_Creating?.Invoke();

            // create a new window
            window = func();

            // register window in ui thread extension
            ThreadExtensions.RegisterWindow(window);

            // register window in ui element extension
            var lifecycleHandler = new WindowLifecycleHandler
            {
                ExitDeferral = deferral,
                LifecycleActions = lifecycleActions ?? new()
            };
            RegisterWindow(window, lifecycleHandler);

            // invoke action after window creation
            lifecycleActions?.Window_Created?.Invoke(window);
        }

        return window;
    }

    public static async Task CloseWindowAsync(Window window)
    {
        if (window == null)
        {
            return;
        }

        var lifecycleHandler = WindowsAndLifecycle.TryGetValue(window, out var value) ? value : null;
        if (lifecycleHandler?.ExitDeferral is not null)
        {
            // initialize task completion source
            lifecycleHandler.LifecycleActions.CompletionSource ??= new();

            // start dispatch complete deferral
            lifecycleHandler.ExitDeferral.Complete();

            // wait for task completion source to complete
            await lifecycleHandler.LifecycleActions.CompletionSource.Task;
        }
        else
        {
            // invoke action before window closing
            lifecycleHandler?.LifecycleActions.Window_Closing?.Invoke(window);

            // close window
            window.Close();

            // invoke action after window closing
            lifecycleHandler?.LifecycleActions.Window_Closed?.Invoke();
        }
    }

    public static async Task CloseAllWindowsAsync()
    {
        foreach (var window in WindowsAndLifecycle.Keys)
        {
            await CloseWindowAsync(window);
        }
        CurrentParameter = null;
    }

    #endregion

    #region register & unregister

    private static void RegisterWindow(Window window, WindowLifecycleHandler lifecycleHandler)
    {
        if (WindowsAndLifecycle.TryAdd(window, lifecycleHandler))
        {
            window.Closed += (sender, args) => UnregisterWindow(window);
        }
    }

    private static void UnregisterWindow(Window window)
    {
        WindowsAndLifecycle.Remove(window);
    }

    #endregion

    private class WindowLifecycleHandler
    {
        public DispatcherExitDeferral? ExitDeferral { get; set; }

        public WindowLifecycleActions LifecycleActions { get; set; } = null!;
    }

    public class WindowLifecycleActions
    {
        public TaskCompletionSource? CompletionSource { get; set; }

        public Action? Window_Creating { get; set; }

        public Action<Window>? Window_Created { get; set; }

        public Action<Window>? Window_Closing { get; set; }

        public Action? Window_Closed { get; set; }
    }
}
