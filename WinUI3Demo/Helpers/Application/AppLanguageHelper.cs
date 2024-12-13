// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using System.Collections.ObjectModel;
using System.Globalization;
using WinGlobalization = Windows.Globalization;  // For packaged apps
using MicGlobalization = Microsoft.Windows.Globalization;  // For unpackaged apps

namespace WinUI3Demo.Helpers.Application;

/// <summary>
/// Provides static helper to manage supported languages in the application.
/// </summary>
public static class AppLanguageHelper
{
    /// <summary>
    /// A constant string representing the default language code.
    /// It is initialized as an empty string.
    /// </summary>
    public static readonly string DefaultCode = string.Empty;

    /// <summary>
    /// A collection of available languages.
    /// </summary>
    public static ObservableCollection<AppLanguageItem> SupportedLanguages { get; private set; } = null!;

    /// <summary>
    /// Gets the preferred language.
    /// </summary>
    public static AppLanguageItem PreferredLanguage { get; private set; } = null!;

    /// <summary>
    /// A collection of manifest languages.
    /// </summary>
    private static IReadOnlyList<string> _manifestLanguages = null!;

    /// <summary>
    /// Initializes the <see cref="AppLanguageHelper"/> class.
    /// </summary>
    public static void Initialize()
    {
        // Get the manifest languages
        if (RuntimeHelper.IsMSIX)
        {
            _manifestLanguages = WinGlobalization.ApplicationLanguages.ManifestLanguages;
        }
        else
        {
            // TODO: Add the languages supported by the unpackaged app
            _manifestLanguages =
            [
                "en-US"
            ];
        }

        // Populate the Languages collection with available languages
        var appLanguages = _manifestLanguages
           .Append(string.Empty) // Add default language code
           .Select(language => new AppLanguageItem(language))
           .OrderBy(language => language.Code is not "") // Default language on top
           .ThenBy(language => language.Name)
           .ToList();

        // Get the current primary language override.
        AppLanguageItem? current;
        if (RuntimeHelper.IsMSIX)
        {
            current = new AppLanguageItem(WinGlobalization.ApplicationLanguages.PrimaryLanguageOverride);
        }
        else
        {
            current = new AppLanguageItem(MicGlobalization.ApplicationLanguages.PrimaryLanguageOverride);
        }

        // Find the index of the saved language
        var index = appLanguages.IndexOf(appLanguages.FirstOrDefault(dl => dl.Name == current.Name) ?? appLanguages.First());

        // Set the system default language as the first item in the Languages collection
        var systemLanguage = new AppLanguageItem(CultureInfo.InstalledUICulture.Name, systemDefault: true);
        if (appLanguages.Select(lang => lang.Name.Contains(systemLanguage.Name)).Any())
        {
            appLanguages[0] = systemLanguage;
        }
        else
        {
            appLanguages[0] = new("en-US", systemDefault: true);
        }

        // Initialize the list
        SupportedLanguages = new(appLanguages);
        PreferredLanguage = SupportedLanguages[index];

        // Set the primary language override
        if (RuntimeHelper.IsMSIX)
        {
            // No need to set in packaged app - it is already set by the app
        }
        else
        {
            var primaryLanguageOverride = DependencyExtensions.GetRequiredService<IAppSettingsService>().Language;
            TryChange(primaryLanguageOverride);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="languageName"></param>
    /// <returns></returns>
    public static string GetLanguageCode(int index)
    {
        if (index >= SupportedLanguages.Count)
        {
            return DefaultCode;
        }

        return index == 0 ? DefaultCode : SupportedLanguages[index].Code;
    }

    /// <summary>
    /// Attempts to change the preferred language code by index.
    /// </summary>
    /// <param name="index">The index of the new language.</param>
    /// <returns>True if the language was successfully changed; otherwise, false.</returns>
    public static bool TryChange(int index)
    {
        if (index >= SupportedLanguages.Count || PreferredLanguage == SupportedLanguages[index])
        {
            return false;
        }

        PreferredLanguage = SupportedLanguages[index];

        // Update the primary language override
        if (RuntimeHelper.IsMSIX)
        {
            WinGlobalization.ApplicationLanguages.PrimaryLanguageOverride = index == 0 ? DefaultCode : PreferredLanguage.Code;
        }
        else
        {
            MicGlobalization.ApplicationLanguages.PrimaryLanguageOverride = index == 0 ? DefaultCode : PreferredLanguage.Code;
        }

        return true;
    }

    /// <summary>
    /// Attempts to change the preferred language code by code.
    /// </summary>
    /// <param name="code">The code of the new language.</param>
    /// <returns>True if the language was successfully changed; otherwise, false.</returns>
    public static bool TryChange(string code)
    {
        var lang = new AppLanguageItem(code);
        var find = SupportedLanguages.FirstOrDefault(dl => dl.Name == lang.Name);
        if (find is null)
        {
            return false;
        }

        // Find the index of the language
        var index = SupportedLanguages.IndexOf(find);

        if (PreferredLanguage == SupportedLanguages[index])
        {
            return false;
        }

        PreferredLanguage = SupportedLanguages[index];

        // Update the primary language override
        if (RuntimeHelper.IsMSIX)
        {
            WinGlobalization.ApplicationLanguages.PrimaryLanguageOverride = index == 0 ? DefaultCode : PreferredLanguage.Code;
        }
        else
        {
            MicGlobalization.ApplicationLanguages.PrimaryLanguageOverride = index == 0 ? DefaultCode : PreferredLanguage.Code;
        }

        return true;
    }
}
