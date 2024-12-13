namespace WinUI3Demo.Core.Contracts.Services;

public interface IDependencyService
{
    T GetService<T>() where T : class;
}
