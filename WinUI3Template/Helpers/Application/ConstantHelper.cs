namespace WinUI3Template.Helpers.Application;

public class ConstantHelper
{
#if DEBUG
    public static readonly string AppDisplayName = "AppDisplayName".GetLocalizedString() + " (Debug)";
#else
    public static readonly string AppDisplayName = "AppDisplayName".GetLocalized();
#endif
}
