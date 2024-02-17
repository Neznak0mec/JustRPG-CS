using System.Globalization;

namespace JustRPG.Models.Enums;

public enum Language
{
    ru,
    en
}

public static class LanguageExtensions
{
    public static void UseLanguage(this Language language)
    {
        CultureInfo.CurrentCulture = language switch
        {
            Language.ru => new CultureInfo("ru"),
            Language.en => new CultureInfo("en"),
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }
}