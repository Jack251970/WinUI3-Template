namespace WinUI3Template.Services;

internal class DependencyService : IDependencyService
{
    public T GetService<T>() where T : class
    {
        return App.GetService<T>();
    }
}
