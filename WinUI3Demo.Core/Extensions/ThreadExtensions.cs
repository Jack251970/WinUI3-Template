// Copyright (c) 2024 Jack251970
// Licensed under the MIT License. See the LICENSE.

using System.Runtime.InteropServices;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Serilog;

namespace WinUI3Demo.Core.Extensions;

/// <summary>
/// Provides static extension for threads.
/// Codes in 'ignore exceptions' are edited from https://github.com/files-community/Files.
/// </summary>
public static class ThreadExtensions
{
    private static readonly ILogger _log = Log.ForContext("SourceContext", nameof(ThreadExtensions));

    private static readonly Dictionary<Window, int> WindowsAndDispatcherThreads = [];

    #region register & unregister

    public static void RegisterWindow<T>(T window) where T : Window
    {
        var threadId = Environment.CurrentManagedThreadId;
        if (!WindowsAndDispatcherThreads.TryAdd(window, threadId))
        {
            WindowsAndDispatcherThreads[window] = threadId;
        }

        window.Closed += (sender, args) => UnregisterWindow(window);
    }

    private static void UnregisterWindow<T>(T window) where T : Window
    {
        WindowsAndDispatcherThreads.Remove(window);
    }

    #endregion

    #region ui thread extensions

    #region single window

    public static Task EnqueueOrInvokeAsync<T>(this T window, Func<T, Task> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) where T : Window
    {
        return IgnoreExceptions(() =>
        {
            var dispatcher = window.DispatcherQueue;
            if (dispatcher is not null && window.IsDispatcherThreadDifferent())
            {
                return dispatcher.EnqueueAsync(() => function(window), priority);
            }
            else
            {
                return function(window);
            }
        }, typeof(COMException));
    }

    public static Task<T1?> EnqueueOrInvokeAsync<T, T1>(this T window, Func<T, Task<T1>> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) where T : Window
    {
        return IgnoreExceptions(() =>
        {
            var dispatcher = window.DispatcherQueue;
            if (dispatcher is not null && window.IsDispatcherThreadDifferent())
            {
                return dispatcher.EnqueueAsync(() => function(window), priority);
            }
            else
            {
                return function(window);
            }
        }, typeof(COMException));
    }

    public static Task EnqueueOrInvokeAsync<T>(this T window, Action<T> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) where T : Window
    {
        return IgnoreExceptions(() =>
        {
            var dispatcher = window.DispatcherQueue;
            if (dispatcher is not null && window.IsDispatcherThreadDifferent())
            {
                return dispatcher.EnqueueAsync(() => function(window), priority);
            }
            else
            {
                function(window);
                return Task.CompletedTask;
            }
        }, typeof(COMException));
    }

    public static Task<T1?> EnqueueOrInvokeAsync<T, T1>(this T window, Func<T, T1> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) where T : Window
    {
        return IgnoreExceptions(() =>
        {
            var dispatcher = window.DispatcherQueue;
            if (dispatcher is not null && window.IsDispatcherThreadDifferent())
            {
                return dispatcher.EnqueueAsync(() => function(window), priority);
            }
            else
            {
                return Task.FromResult(function(window));
            }
        }, typeof(COMException));
    }

    #endregion

    #region multiple windows

    public static async Task EnqueueOrInvokeAsync<T>(this List<T> windows, Func<T, Task> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) where T : Window
    {
        var tasks = new List<Task>();

        foreach (var window in windows)
        {
            tasks.Add(window.EnqueueOrInvokeAsync(function, priority));
        }

        await Task.WhenAll(tasks);
    }

    public static async Task EnqueueOrInvokeAsync<T>(this List<T> windows, Action<T> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) where T : Window
    {
        var tasks = new List<Task>();

        foreach (var window in windows)
        {
            tasks.Add(window.EnqueueOrInvokeAsync(function, priority));
        }

        await Task.WhenAll(tasks);
    }

    #endregion

    #region ignore exceptions

    private static async Task<bool> IgnoreExceptions(Func<Task> action, Type? exceptionToIgnore = null)
    {
        try
        {
            await action();

            return true;
        }
        catch (Exception ex)
        {
            if (exceptionToIgnore is null || exceptionToIgnore.IsAssignableFrom(ex.GetType()))
            {
                _log.Error(ex, $"{ExceptionFormatter.FormatExcpetion(ex)}");

                return false;
            }
            else
            {
                throw;
            }
        }
    }

    private static async Task<T?> IgnoreExceptions<T>(Func<Task<T>> action, Type? exceptionToIgnore = null)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            if (exceptionToIgnore is null || exceptionToIgnore.IsAssignableFrom(ex.GetType()))
            {
                _log.Error(ex, $"{ExceptionFormatter.FormatExcpetion(ex)}");

                return default;
            }
            else
            {
                throw;
            }
        }
    }

    #endregion

    #region helper methods

    private static bool IsDispatcherThreadDifferent<T>(this T window) where T : Window
    {
        return Environment.CurrentManagedThreadId != window.GetDispatcherThreadId();
    }

    private static int GetDispatcherThreadId<T>(this T window) where T : Window
    {
        return WindowsAndDispatcherThreads.FirstOrDefault(x => x.Key == window).Value;
    }

    #endregion

    #endregion
}
