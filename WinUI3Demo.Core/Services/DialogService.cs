using Windows.UI.Popups;
using WinUIEx;

namespace WinUI3Demo.Core.Services;

public class DialogService : IDialogService
{
    private DialogScreenWindow DialogScreen = null!;

    private readonly string Ok = "ButtonOk.Content".GetLocalizedString();
    private readonly string Cancel = "ButtonCancel.Content".GetLocalizedString();

    public void Initialize()
    {
        DialogScreen = WindowsExtensions.CreateWindow<DialogScreenWindow>();
    }

    #region WindowEx Dialog

    #region Public

    public async Task ShowOneButtonDialogAsync(WindowEx window, string title, string context)
    {
        await ShowMessageDialogAsync(window, context, null, title: title);
    }

    public async Task<WidgetDialogResult> ShowTwoButtonDialogAsync(WindowEx window, string title, string context, string leftButton = null!, string rightButton = null!)
    {
        leftButton = leftButton is null ? Ok : leftButton;
        rightButton = rightButton is null ? Cancel : rightButton;
        var commands = new List<IUICommand>
        {
            new UICommand(leftButton),
            new UICommand(rightButton)
        };

        var result = await ShowMessageDialogAsync(window, context, commands, title: title);

        if (result == null)
        {
            return WidgetDialogResult.Unknown;
        }

        if (result.Label == leftButton)
        {
            return WidgetDialogResult.Left;
        }
        else
        {
            return WidgetDialogResult.Right;
        }
    }

    public async Task<WidgetDialogResult> ShowThreeButtonDialogAsync(WindowEx window, string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!)
    {
        if (centerButton is null)
        {
            return await ShowTwoButtonDialogAsync(window, title, context, leftButton, rightButton);
        }

        leftButton = leftButton is null ? Ok : leftButton;
        rightButton = rightButton is null ? Cancel : rightButton;
        var commands = new List<IUICommand>
        {
            new UICommand(leftButton),
            new UICommand(centerButton),
            new UICommand(rightButton)
        };

        var result = await ShowMessageDialogAsync(window, context, commands, title: title);

        if (result == null)
        {
            return WidgetDialogResult.Unknown;
        }

        if (result.Label == leftButton)
        {
            return WidgetDialogResult.Left;
        }
        else if (result.Label == centerButton)
        {
            return WidgetDialogResult.Center;
        }
        else
        {
            return WidgetDialogResult.Right;
        }
    }

    #endregion

    #region Private

    private static async Task<IUICommand?> ShowMessageDialogAsync(WindowEx window, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0u, uint cancelCommandIndex = 1u, string title = "")
    {
        return await window.ShowMessageDialogAsync(content, commands, defaultCommandIndex, cancelCommandIndex, title);
    }

    #endregion

    #endregion

    #region Full Screen Dialog

    #region Public

    public async Task ShowFullScreenOneButtonDialogAsync(string title, string context)
    {
        await ShowFullScreenMessageDialogAsync(context, null, title: title);
    }

    public async Task ShowFullScreenTwoButtonDialogAsync(string title, string context, string leftButton = null!, string rightButton = null!, Action<WidgetDialogResult>? func = null)
    {
        leftButton = leftButton is null ? Ok : leftButton;
        rightButton = rightButton is null ? Cancel : rightButton;
        var commands = new List<IUICommand>
        {
            new UICommand(leftButton),
            new UICommand(rightButton)
        };

        await ShowFullScreenMessageDialogAsync(context, leftButton, null, rightButton, commands, title: title, func: func);
    }

    public async Task ShowFullScreenTwoButtonDialogAsync(string title, string context, string leftButton = null!, string rightButton = null!, Func<WidgetDialogResult, Task>? func = null)
    {
        leftButton = leftButton is null ? Ok : leftButton;
        rightButton = rightButton is null ? Cancel : rightButton;
        var commands = new List<IUICommand>
        {
            new UICommand(leftButton),
            new UICommand(rightButton)
        };

        await ShowFullScreenMessageDialogAsync(context, leftButton, null, rightButton, commands, title: title, func: func);
    }

    public async Task ShowFullScreenThreeButtonDialogAsync(string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!, Action<WidgetDialogResult>? func = null)
    {
        if (centerButton is null)
        {
            await ShowFullScreenTwoButtonDialogAsync(title, context, leftButton, rightButton, func);
        }

        leftButton = leftButton is null ? Ok : leftButton;
        rightButton = rightButton is null ? Cancel : rightButton;
        var commands = new List<IUICommand>
        {
            new UICommand(leftButton),
            new UICommand(centerButton),
            new UICommand(rightButton)
        };

        await ShowFullScreenMessageDialogAsync(context, leftButton, centerButton, rightButton, commands, title: title, func: func);
    }

    public async Task ShowFullScreenThreeButtonDialogAsync(string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!, Func<WidgetDialogResult, Task>? func = null)
    {
        if (centerButton is null)
        {
            await ShowFullScreenTwoButtonDialogAsync(title, context, leftButton, rightButton, func);
        }

        leftButton = leftButton is null ? Ok : leftButton;
        rightButton = rightButton is null ? Cancel : rightButton;
        var commands = new List<IUICommand>
        {
            new UICommand(leftButton),
            new UICommand(centerButton),
            new UICommand(rightButton)
        };

        await ShowFullScreenMessageDialogAsync(context, leftButton, centerButton, rightButton, commands, title: title, func: func);
    }

    #endregion

    #region Private

    private async Task ShowFullScreenMessageDialogAsync(string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0u, uint cancelCommandIndex = 1u, string title = "")
    {
        await DialogScreen.EnqueueOrInvokeAsync(async (window) =>
        {
            window.Show();
            if (window.Visible)
            {
                await window.ShowMessageDialogAsync(content, commands, defaultCommandIndex, cancelCommandIndex, title);
                window.Hide();
            }
        });
    }

    private async Task ShowFullScreenMessageDialogAsync(string content, string leftButton, string? centerButton, string rightButton, IList<IUICommand>? commands, uint defaultCommandIndex = 0u, uint cancelCommandIndex = 1u, string title = "", Action<WidgetDialogResult>? func = null)
    {
        await DialogScreen.EnqueueOrInvokeAsync(async (window) =>
        {
            window.Show();
            if (window.Visible)
            {
                // get result
                var result = await window.ShowMessageDialogAsync(content, commands, defaultCommandIndex, cancelCommandIndex, title);
                window.Hide();

                // invoke function
                if (func != null)
                {
                    if (result == null)
                    {
                        func.Invoke(WidgetDialogResult.Unknown);
                    }
                    else if (result.Label == leftButton)
                    {
                        func.Invoke(WidgetDialogResult.Left);
                    }
                    else if (result.Label == rightButton)
                    {
                        func.Invoke(WidgetDialogResult.Right);
                    }
                    else if (centerButton != null)
                    {
                        func.Invoke(WidgetDialogResult.Center);
                    }
                    else
                    {
                        func.Invoke(WidgetDialogResult.Unknown);
                    }
                }
            }
        });
    }

    private async Task ShowFullScreenMessageDialogAsync(string content, string leftButton, string? centerButton, string rightButton, IList<IUICommand>? commands, uint defaultCommandIndex = 0u, uint cancelCommandIndex = 1u, string title = "", Func<WidgetDialogResult, Task>? func = null)
    {
        await DialogScreen.EnqueueOrInvokeAsync(async (window) =>
        {
            window.Show();
            if (window.Visible)
            {
                // get result
                var result = await window.ShowMessageDialogAsync(content, commands, defaultCommandIndex, cancelCommandIndex, title);
                window.Hide();

                // invoke function
                if (func != null)
                {
                    if (result == null)
                    {
                        await func.Invoke(WidgetDialogResult.Unknown);
                    }
                    else if (result.Label == leftButton)
                    {
                        await func.Invoke(WidgetDialogResult.Left);
                    }
                    else if (result.Label == rightButton)
                    {
                        await func.Invoke(WidgetDialogResult.Right);
                    }
                    else if (centerButton != null)
                    {
                        await func.Invoke(WidgetDialogResult.Center);
                    }
                    else
                    {
                        await func.Invoke(WidgetDialogResult.Unknown);
                    }
                }
            }
        });
    }

    #endregion

    #endregion
}

public enum WidgetDialogResult
{
    Left,
    Center,
    Right,
    Unknown
}
