namespace WinUI3Demo.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string viewModel);

    string GetPageKey(Type pageType);
}
