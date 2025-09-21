using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SuGarToolkit.Controls.Dialogs;

namespace WinUI3Template.Core.Services;

public class DialogService : IDialogService
{
    private readonly string Ok = "ButtonOk.Content".GetLocalizedString();
    private readonly string Cancel = "ButtonCancel.Content".GetLocalizedString();

    #region Window Dialog

    #region Public

    public async Task ShowOneButtonDialogAsync(Window window, string title, string context)
    {
        var dialog = new ContentDialog()
        {
            Title = title,
            Content = context,
            PrimaryButtonText = Ok,
            DefaultButton = ContentDialogButton.Primary
        };
        await dialog.ShowAsync();
    }

    public async Task<WidgetDialogResult> ShowTwoButtonDialogAsync(Window window, string title, string context, string leftButton = null!, string rightButton = null!)
    {
        leftButton = string.IsNullOrWhiteSpace(leftButton) ? Ok : leftButton;
        rightButton = string.IsNullOrWhiteSpace(rightButton) ? Cancel : rightButton;
        
        var dialog = new ContentDialog()
        {
            Title = title,
            Content = context,
            PrimaryButtonText = leftButton,
            SecondaryButtonText = rightButton
        };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            return WidgetDialogResult.Left;
        }
        else if (result == ContentDialogResult.Secondary)
        {
            return WidgetDialogResult.Right;
        }
        else
        {
            return WidgetDialogResult.Unknown;
        }
    }

    public async Task<WidgetDialogResult> ShowThreeButtonDialogAsync(Window window, string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!)
    {
        if (string.IsNullOrWhiteSpace(centerButton))
        {
            return await ShowTwoButtonDialogAsync(window, title, context, leftButton, rightButton);
        }

        leftButton = string.IsNullOrWhiteSpace(leftButton) ? Ok : leftButton;
        rightButton = string.IsNullOrWhiteSpace(rightButton) ? Cancel : rightButton;

        var dialog = new ContentDialog()
        {
            Title = title,
            Content = context,
            PrimaryButtonText = leftButton,
            SecondaryButtonText = centerButton,
            CloseButtonText = rightButton
        };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            return WidgetDialogResult.Left;
        }
        else if (result == ContentDialogResult.Secondary)
        {
            return WidgetDialogResult.Right;
        }
        else if (result == ContentDialogResult.None)
        {
            return WidgetDialogResult.Right;
        }
        else
        {
            return WidgetDialogResult.Unknown;
        }
    }

    #endregion

    #endregion

    #region Full Screen Dialog

    #region Public

    public async Task ShowFullScreenOneButtonDialogAsync(string title, string context)
    {
        var dialog = new WindowedContentDialog()
        {
            WindowTitle = title,
            Title = title,
            Content = context,
            OwnerWindow = null,
            PrimaryButtonText = Ok,
            IsTitleBarVisible = false
        };
        await dialog.ShowAsync();
    }

    public async Task<WidgetDialogResult> ShowFullScreenTwoButtonDialogAsync(string title, string context, string leftButton = null!, string rightButton = null!)
    {
        leftButton = string.IsNullOrWhiteSpace(leftButton) ? Ok : leftButton;
        rightButton = string.IsNullOrWhiteSpace(rightButton) ? Cancel : rightButton;

        var dialog = new WindowedContentDialog()
        {
            WindowTitle = title,
            Title = title,
            Content = context,
            OwnerWindow = null,
            PrimaryButtonText = leftButton,
            SecondaryButtonText = rightButton,
            IsTitleBarVisible = false
        };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            return WidgetDialogResult.Left;
        }
        else if (result == ContentDialogResult.Secondary)
        {
            return WidgetDialogResult.Right;
        }
        else
        {
            return WidgetDialogResult.Unknown;
        }
    }

    public async Task<WidgetDialogResult> ShowFullScreenThreeButtonDialogAsync(string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!)
    {
        if (string.IsNullOrWhiteSpace(centerButton))
        {
            return await ShowFullScreenTwoButtonDialogAsync(title, context, leftButton, rightButton);
        }

        leftButton = string.IsNullOrWhiteSpace(leftButton) ? Ok : leftButton;
        rightButton = string.IsNullOrWhiteSpace(rightButton) ? Cancel : rightButton;

        var dialog = new WindowedContentDialog()
        {
            WindowTitle = title,
            Title = title,
            Content = context,
            OwnerWindow = null,
            PrimaryButtonText = leftButton,
            SecondaryButtonText = centerButton,
            CloseButtonText = rightButton,
            IsTitleBarVisible = false
        };
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            return WidgetDialogResult.Left;
        }
        else if (result == ContentDialogResult.Secondary)
        {
            return WidgetDialogResult.Right;
        }
        else if (result == ContentDialogResult.None)
        {
            return WidgetDialogResult.Right;
        }
        else
        {
            return WidgetDialogResult.Unknown;
        }
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
