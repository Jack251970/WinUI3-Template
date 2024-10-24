using WinUIEx;

namespace WinUI3Template.Core.Contracts.Services;

public interface IDialogService
{
    void Initialize();

    Task ShowOneButtonDialogAsync(WindowEx window, string title, string context);

    Task<WidgetDialogResult> ShowTwoButtonDialogAsync(WindowEx window, string title, string context, string leftButton = null!, string rightButton = null!);

    Task<WidgetDialogResult> ShowThreeButtonDialogAsync(WindowEx window, string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!);

    Task ShowFullScreenOneButtonDialogAsync(string title, string context);

    Task ShowFullScreenTwoButtonDialogAsync(string title, string context, string leftButton = null!, string rightButton = null!, Action<WidgetDialogResult>? func = null);

    Task ShowFullScreenTwoButtonDialogAsync(string title, string context, string leftButton = null!, string rightButton = null!, Func<WidgetDialogResult, Task>? func = null);

    Task ShowFullScreenThreeButtonDialogAsync(string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!, Action<WidgetDialogResult>? func = null);

    Task ShowFullScreenThreeButtonDialogAsync(string title, string context, string leftButton = null!, string centerButton = null!, string rightButton = null!, Func<WidgetDialogResult, Task>? func = null);
}
