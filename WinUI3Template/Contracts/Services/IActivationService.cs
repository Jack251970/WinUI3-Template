using Microsoft.UI.Xaml;

namespace WinUI3Template.Contracts.Services;

public interface IActivationService
{
#if SPLASH_SCREEN
    Task LaunchMainWindowAsync(object activationArgs);
#endif

    Task ActivateMainWindowAsync(object activationArgs);

    Task ActivateWindowAsync(Window window);
}
