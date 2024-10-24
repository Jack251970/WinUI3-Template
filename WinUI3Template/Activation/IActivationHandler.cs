namespace WinUI3Template.Activation;

internal interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}
