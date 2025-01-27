using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinUI3Demo.ViewModels.Pages;

public partial class HomeViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private string _appDisplayName = ConstantHelper.AppDisplayName;

    [ObservableProperty]
    private List<Vector2> _salesData;

    [ObservableProperty]
    private int _sSS;

    private bool changing;

    partial void OnSSSChanged(int value)
    {
        if (changing) return;

        changing = true;
        SSS = SSS + 1;
        changing = false;
    }

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

    public void OnNavigatedTo(object parameter)
    {
        Console.WriteLine("OnNavigatedTo");
    }

    public void OnNavigatedFrom()
    {
        Console.WriteLine("OnNavigatedFrom");
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
