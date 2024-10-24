using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
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
        protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
            => compositor.CreateHostBackdropBrush();
    }
}
