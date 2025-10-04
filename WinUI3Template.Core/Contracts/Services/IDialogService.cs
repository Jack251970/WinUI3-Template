using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using WinUI3Template.Core.Services;

namespace WinUI3Template.Core.Contracts.Services;

public interface IDialogService
{
    Task ShowOneButtonDialogAsync(Window window, string title, string content);

    Task<WidgetDialogResult> ShowTwoButtonDialogAsync(Window window, string title, string content, string leftButton = null!, string rightButton = null!);

    Task<WidgetDialogResult> ShowThreeButtonDialogAsync(Window window, string title, string content, string leftButton = null!, string centerButton = null!, string rightButton = null!);

    Task ShowFullScreenOneButtonDialogAsync(string title, string content);

    Task<WidgetDialogResult> ShowFullScreenTwoButtonDialogAsync(string title, string content, string leftButton = null!, string rightButton = null!);

    Task<WidgetDialogResult> ShowFullScreenThreeButtonDialogAsync(string title, string content, string leftButton = null!, string centerButton = null!, string rightButton = null!);
}
