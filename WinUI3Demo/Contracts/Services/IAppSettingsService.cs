using Microsoft.UI.Xaml;

namespace WinUI3Demo.Contracts.Services;

public interface IAppSettingsService
{
    void Initialize();

    string Language { get; }

    Task SaveLanguageInSettingsAsync(string language);

    ElementTheme Theme { get; }

    Task SaveThemeInSettingsAsync(ElementTheme theme);

    BackdropType BackdropType { get; }

    Task SaveBackdropTypeInSettingsAsync(BackdropType type);
}
