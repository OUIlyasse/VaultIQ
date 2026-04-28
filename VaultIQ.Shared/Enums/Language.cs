namespace VaultIQ.Shared.Enums;

/// <summary>
/// Langues supportées par VaultIQ.
/// Arabic déclenche automatiquement le mode RTL (Right-To-Left).
/// </summary>
public enum Language
{
    /// <summary>Français 🇫🇷 — langue par défaut.</summary>
    French = 0,

    /// <summary>English 🇬🇧</summary>
    English = 1,

    /// <summary>العربية 🇸🇦 — avec support RTL complet.</summary>
    Arabic = 2
}

public static class LanguageExtensions
{
    /// <summary>Retourne true si la langue s'écrit de droite à gauche.</summary>
    public static bool IsRightToLeft(this Language lang) => lang == Language.Arabic;

    /// <summary>Affichage natif de la langue pour l'UI de sélection.</summary>
    public static string ToDisplayName(this Language lang) => lang switch
    {
        Language.French => "🇫🇷  Français",
        Language.English => "🇬🇧  English",
        Language.Arabic => "🇸🇦  العربية",
        _ => lang.ToString()
    };

    /// <summary>Code ISO 639-1 de la langue.</summary>
    public static string ToIsoCode(this Language lang) => lang switch
    {
        Language.French => "fr",
        Language.English => "en",
        Language.Arabic => "ar",
        _ => "fr"
    };
}