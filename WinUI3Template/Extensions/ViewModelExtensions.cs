using Microsoft.UI.Xaml.Controls;

namespace WinUI3Template.Extensions;

internal static class ViewModelExtensions
{
    public static object? GetPageViewModel(this Frame frame) =>
        frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content, null);
}
