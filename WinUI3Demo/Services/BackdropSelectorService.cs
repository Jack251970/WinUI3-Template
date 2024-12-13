using Microsoft.UI.Xaml;

namespace WinUI3Demo.Services;

internal class BackdropSelectorService(IAppSettingsService appSettingsService) : IBackdropSelectorService
{
    public BackdropType BackdropType => _appSettingsService.BackdropType;

    public event EventHandler<BackdropType>? BackdropTypeChanged;

    private readonly IAppSettingsService _appSettingsService = appSettingsService;

    public async Task SetBackdropTypeAsync(BackdropType type)
    {
        await SetRequestedBackdropTypeAsync(App.MainWindow, type);

        await WindowsExtensions.GetAllWindows().EnqueueOrInvokeAsync(
            async (window) => await SetRequestedBackdropTypeAsync(window, type), 
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal);

        BackdropTypeChanged?.Invoke(this, type);

        await _appSettingsService.SaveBackdropTypeInSettingsAsync(type);
    }

    public async Task SetRequestedBackdropTypeAsync(Window window)
    {
        await SetRequestedBackdropTypeAsync(window, BackdropType);
    }

    private static async Task SetRequestedBackdropTypeAsync(Window window, BackdropType type)
    {
        BackdropHelper.SetRequestedBackdropAsync(window, type);

        await Task.CompletedTask;
    }
}
