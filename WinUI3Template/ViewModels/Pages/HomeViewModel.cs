using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI3Template.ViewModels.Pages;

public partial class HomeViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _appDisplayName = ConstantHelper.AppDisplayName;

    public HomeViewModel()
    {

    }
}
