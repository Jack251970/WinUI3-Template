namespace WinUI3Template.Contracts.Services;

public interface IPageService
{
    Type SettingPageType { get; }

    string SettingPageKey { get; }

    Type GetPageType(string viewModel);

    string GetPageKey(Type pageType);

    string? GetSubpageKey(Type pageType);
}
