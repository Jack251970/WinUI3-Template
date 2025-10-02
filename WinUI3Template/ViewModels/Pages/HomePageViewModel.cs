using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI3Template.ViewModels.Pages;

public partial class HomePageViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string AppDisplayName { get; set; } = ConstantHelper.AppDisplayName;

    public HomePageViewModel()
    {

    }
}
