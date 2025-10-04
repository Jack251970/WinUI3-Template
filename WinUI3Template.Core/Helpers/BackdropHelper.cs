using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Composition;
using WinUI3Template.Core.Models;
using WinUIEx;

namespace WinUI3Template.Core.Helpers;

public partial class BackdropHelper
{
    public static void SetRequestedBackdropAsync(Window window, BackdropType type)
    {
        if (window != null)
        {
            window.SystemBackdrop = type switch
            {
                BackdropType.None => null,
                BackdropType.Acrylic => new DesktopAcrylicBackdrop(),
                BackdropType.Blur => new BlurredBackdrop(),
                BackdropType.Transparent => new TransparentTintBackdrop(),
                _ => new MicaBackdrop(),
            };
        }
    }

    private partial class BlurredBackdrop : CompositionBrushBackdrop
    {
        protected override CompositionBrush CreateBrush(Compositor compositor)
            => compositor.CreateHostBackdropBrush();
    }
}
