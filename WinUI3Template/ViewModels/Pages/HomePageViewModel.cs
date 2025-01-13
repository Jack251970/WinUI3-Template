using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI3Template.ViewModels.Pages;

public partial class HomePageViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _appDisplayName = ConstantHelper.AppDisplayName;

    public HomePageViewModel()
    {

    }
}
