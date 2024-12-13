namespace WinUI3Demo.Activation;

internal interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}
