using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Extensions;

internal static class ViewModelExtensions
{
    public static object? GetPageViewModel(this Frame frame)
    {
        if (frame?.Content is not null)
        {
            return (frame?.Content as FrameworkElement)?.DataContext ??
                throw new InvalidOperationException("The page does not have a DataContext.");
        }

        return null;
    }
}
