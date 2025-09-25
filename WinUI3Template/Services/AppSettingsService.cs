using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;

namespace WinUI3Template.Services;

internal class AppSettingsService(ILocalSettingsService localSettingsService, IOptions<LocalSettingsKeys> localSettingsKeys) : IAppSettingsService
{
    private readonly ILocalSettingsService _localSettingsService = localSettingsService;
    private readonly LocalSettingsKeys _localSettingsKeys = localSettingsKeys.Value;

    private bool _isInitialized;

    public void Initialize()
    {
        if (!_isInitialized)
        {
            // initialize local settings
            Language = GetLanguage();
            Theme = GetTheme();
            BackdropType = GetBackdropType();

            _isInitialized = true;
        }
    }

    #region Language

    private string language = DefaultLanguage;
    public string Language
    {
        get => language;
        private set
        {
            if (language != value)
            {
                language = value;
            }
        }
    }

    private static readonly string DefaultLanguage = AppLanguageHelper.DefaultCode;

    private string GetLanguage()
    {
        var data = GetDataFromSettings(_localSettingsKeys.LanguageKey, DefaultLanguage);
        return data;
    }

    public async Task SaveLanguageInSettingsAsync(string language)
    {
        await SaveDataInSettingsAsync(_localSettingsKeys.LanguageKey, language);
        Language = language;
    }

    #endregion

    #region Theme

    private ElementTheme theme = DefaultTheme;
    public ElementTheme Theme
    {
        get => theme;
        private set
        {
            if (theme != value)
            {
                theme = value;
            }
        }
    }

    private const ElementTheme DefaultTheme = ElementTheme.Default;

    private ElementTheme GetTheme()
    {
        var data = GetDataFromSettings(_localSettingsKeys.ThemeKey, DefaultTheme);
        return data;
    }

    public async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        await SaveDataInSettingsAsync(_localSettingsKeys.ThemeKey, theme);
        Theme = theme;
    }

    #endregion

    #region Backdrop

    private BackdropType backdropType = DefaultBackdropType;
    public BackdropType BackdropType
    {
        get => backdropType;
        private set
        {
            if (backdropType != value)
            {
                backdropType = value;
            }
        }
    }

    private const BackdropType DefaultBackdropType = BackdropType.Mica;

    private BackdropType GetBackdropType()
    {
        var data = GetDataFromSettings(_localSettingsKeys.BackdropTypeKey, DefaultBackdropType);
        return data;
    }

    public async Task SaveBackdropTypeInSettingsAsync(BackdropType type)
    {
        await SaveDataInSettingsAsync(_localSettingsKeys.BackdropTypeKey, type);
        BackdropType = type;
    }

    #endregion

    #region Helper Methods

    private T GetDataFromSettings<T>(string settingsKey, T defaultData)
    {
        var data = _localSettingsService.ReadSetting<string>(settingsKey);

        if (typeof(T) == typeof(bool) && bool.TryParse(data, out var cacheBoolData))
        {
            return (T)(object)cacheBoolData;
        }
        else if (typeof(T) == typeof(int) && int.TryParse(data, out var cacheIntData))
        {
            return (T)(object)cacheIntData;
        }
        else if (typeof(T) == typeof(DateTime) && DateTime.TryParse(data, out var cacheDateTimeData))
        {
            return (T)(object)cacheDateTimeData;
        }
        else if (typeof(T) == typeof(string) && data?.ToString() is string cacheStringData)
        {
            return (T)(object)cacheStringData;
        }
        else if (typeof(T).IsEnum && Enum.TryParse(typeof(T), data, out var cacheEnumData))
        {
            return (T)cacheEnumData;
        }

        return defaultData;
    }

    private async Task SaveDataInSettingsAsync<T>(string settingsKey, T data)
    {
        await _localSettingsService.SaveSettingAsync(settingsKey, data!.ToString());
    }

    #endregion
}
