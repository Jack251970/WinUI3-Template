using Microsoft.UI.Xaml;

namespace WinUI3Template.Core.Contracts.Services;

public interface IBackdropSelectorService
{
    BackdropType BackdropType { get; }

    public event EventHandler<BackdropType>? BackdropTypeChanged;

    Task SetRequestedBackdropTypeAsync(Window window);

    Task SetBackdropTypeAsync(BackdropType type);
}
