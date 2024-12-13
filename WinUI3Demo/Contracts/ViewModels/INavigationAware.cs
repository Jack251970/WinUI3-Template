namespace WinUI3Demo.Contracts.ViewModels;

internal interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}
