using Microsoft.UI.Xaml;

namespace WinUI3Template.Core.Contracts.Services;

public interface IDialogService
{
    Task ShowOneButtonDialogAsync(Window window, string title, string context);

    Task<WidgetDialogResult> ShowTwoButtonDialogAsync(Window window, string title, string context, string leftButton = null!, string rightButton = null!);

    Task<WidgetDialogResult> ShowThreeButtonDialogAsync(Window window, string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!);

    Task ShowFullScreenOneButtonDialogAsync(string title, string context);

    Task<WidgetDialogResult> ShowFullScreenTwoButtonDialogAsync(string title, string context, string leftButton = null!, string rightButton = null!);

    Task<WidgetDialogResult> ShowFullScreenThreeButtonDialogAsync(string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!);
}
