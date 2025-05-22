using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// This script handles language selection for the game using Unity Localization package.
public class LocalizationManager : MonoBehaviour
{
    // Reference to the UI buttons used to change language
    [SerializeField] private Button Turkish_B;
    [SerializeField] private Button English_B;

    // Called when the script instance is being loaded (before Start)
    private void Awake()
    {
        // When Turkish button is clicked, set the language to Turkish
        Turkish_B.onClick.AddListener(() => SetLanguage(LanguageEnum.Turkish));

        // When English button is clicked, set the language to English
        English_B.onClick.AddListener(() => SetLanguage(LanguageEnum.English));
    }

    // Sets the game language based on the selected enum value
    public void SetLanguage(LanguageEnum languageType)
    {
        // Iterate through all available locales defined in the Localization Settings
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            // Compare the locale name with the selected language enum name
            if (locale.LocaleName.Equals(languageType.ToString()))
            {
                // Set the selected locale, which updates all localized content
                LocalizationSettings.SelectedLocale = locale;
            }
        }
    }
}