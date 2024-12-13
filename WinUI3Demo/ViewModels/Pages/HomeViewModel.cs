using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinUI3Demo.ViewModels.Pages;

public partial class HomeViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _appDisplayName = ConstantHelper.AppDisplayName;

    [ObservableProperty]
    private List<Vector2> _salesData;

    public HomeViewModel()
    {
        SalesData =
        [
            new Vector2() { X = 30f, Y = 0f },
            new Vector2() { X = 56f, Y = 1800f },
            new Vector2() { X = 59f, Y = 2400f },
            new Vector2() { X = 98f, Y = 4800f },
        ];
    }

    partial void OnSalesDataChanged(List<Vector2> value)
    {

    }

    [RelayCommand]
    private void Restore()
    {
        SalesData =
        [
            new Vector2() { X = 30f, Y = 0f },
            new Vector2() { X = 56f, Y = 1800f },
            new Vector2() { X = 59f, Y = 2400f },
            new Vector2() { X = 98f, Y = 4800f },
        ];
    }
}
