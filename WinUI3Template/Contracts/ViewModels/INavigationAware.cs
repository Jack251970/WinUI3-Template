namespace WinUI3Template.Contracts.ViewModels;

internal interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}
