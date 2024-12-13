using Newtonsoft.Json;
using Windows.Storage;

namespace WinUI3Demo.Core.Services;

// For MSIX package:
// Settings saved in C:\Users\<UserName>\AppData\Local\Packages\<PackageFamilyName>\Settings\settings.dat
// File saved in C:\Users\<UserName>\AppData\Local\Packages\<PackageFamilyName>\LocalState\{FileName}
public class LocalSettingsService : ILocalSettingsService
{
    private readonly IFileService _fileService;

    private readonly string _localSettingsPath;

    private readonly string _localsettingsFile;

    private Dictionary<string, object>? _settings;

    private bool _isInitialized;

    public LocalSettingsService(IFileService fileService)
    {
        _fileService = fileService;

        _localsettingsFile = Constants.SettingsFile;

        _localSettingsPath = Path.Combine(LocalSettingsHelper.ApplicationDataPath, Constants.SettingsFolder);

        if (!Directory.Exists(_localSettingsPath))
        {
            Directory.CreateDirectory(_localSettingsPath);
        }
    }

    #region App Settings

    public T? ReadSetting<T>(string key)
    {
        return ReadSetting(key, default(T));
    }

    public T? ReadSetting<T>(string key, T defaultValue)
    {
        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return JsonHelper.ToObject<T>((string)obj);
            }
        }
        else
        {
            InitializeSettings();

            if (_settings != null && _settings.TryGetValue(key, out var obj))
            {
                return JsonHelper.ToObject<T>((string)obj);
            }
        }

        return defaultValue;
    }

    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        return await ReadSettingAsync(key, default(T));
    }

    public async Task<T?> ReadSettingAsync<T>(string key, T defaultValue)
    {
        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return await JsonHelper.ToObjectAsync<T>((string)obj);
            }
        }
        else
        {
            await InitializeSettingsAsync();

            if (_settings != null && _settings.TryGetValue(key, out var obj))
            {
                return await JsonHelper.ToObjectAsync<T>((string)obj);
            }
        }

        return defaultValue;
    }

    public async Task SaveSettingAsync<T>(string key, T value)
    {
        var stringValue = JsonHelper.Stringify(value!);

        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj) && ((string)obj) == stringValue)
            {
                return;
            }

            ApplicationData.Current.LocalSettings.Values[key] = stringValue;
        }
        else
        {
            await InitializeSettingsAsync();

            if (_settings != null && _settings.TryGetValue(key, out var obj) && (string)obj == stringValue)
            {
                return;
            }

            _settings![key] = stringValue;

            await _fileService.SaveAsync(_localSettingsPath, _localsettingsFile, _settings, true);
        }
    }

    private void InitializeSettings()
    {
        if (!_isInitialized)
        {
            _settings = _fileService.Read<Dictionary<string, object>>(_localSettingsPath, _localsettingsFile) ?? [];

            _isInitialized = true;
        }
    }

    private async Task InitializeSettingsAsync()
    {
        if (!_isInitialized)
        {
            _settings = await _fileService.ReadAsync<Dictionary<string, object>>(_localSettingsPath, _localsettingsFile) ?? [];

            _isInitialized = true;
        }
    }

    #endregion

    #region Json Files

    public T? ReadJsonFile<T>(string fileName, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        return _fileService.Read<T>(_localSettingsPath, fileName, jsonSerializerSettings) ?? default;
    }

    public async Task<T?> ReadJsonFileAsync<T>(string fileName, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        return await _fileService.ReadAsync<T>(_localSettingsPath, fileName, jsonSerializerSettings) ?? default;
    }

    public async Task SaveJsonFileAsync<T>(string fileName, T value)
    {
        await _fileService.SaveAsync(_localSettingsPath, fileName, value, true);
    }

    #endregion
}
