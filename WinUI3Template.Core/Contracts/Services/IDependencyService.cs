namespace WinUI3Template.Core.Contracts.Services;

public interface IDependencyService
{
    T GetService<T>() where T : class;
}
