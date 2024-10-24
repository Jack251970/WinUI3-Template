using System.Collections.Specialized;

namespace WinUI3Template.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    void TryShow(string payload);

    void RunShow(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
