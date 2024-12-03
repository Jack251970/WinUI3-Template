using Microsoft.UI.Xaml;

namespace WinUI3Template.Contracts.Services;

public interface IActivationService
{
    Task LaunchMainWindowAsync(object activationArgs);

    Task ActivateMainWindowAsync(object activationArgs);

    Task ActivateWindowAsync(Window window);
}
