namespace VaultIQ.Shared.Enums;

/// <summary>
/// Niveaux de force d'un mot de passe, calculés à partir d'un score 0-100.
/// </summary>
public enum PasswordStrengthLevel
{
    /// <summary>Champ vide ou score 0.</summary>
    Empty = 0,

    /// <summary>Score 1-19 — très vulnérable, à changer immédiatement.</summary>
    VeryWeak = 1,

    /// <summary>Score 20-39 — insuffisant pour des données sensibles.</summary>
    Weak = 2,

    /// <summary>Score 40-59 — acceptable pour des comptes peu critiques.</summary>
    Medium = 3,

    /// <summary>Score 60-79 — bon niveau de sécurité.</summary>
    Strong = 4,

    /// <summary>Score 80-100 — excellent, recommandé pour tous les comptes.</summary>
    VeryStrong = 5
}

public static class PasswordStrengthLevelExtensions
{
    /// <summary>Calcule le niveau à partir d'un score numérique 0-100.</summary>
    public static PasswordStrengthLevel FromScore(int score) => score switch
    {
        0 => PasswordStrengthLevel.Empty,
        <= 19 => PasswordStrengthLevel.VeryWeak,
        <= 39 => PasswordStrengthLevel.Weak,
        <= 59 => PasswordStrengthLevel.Medium,
        <= 79 => PasswordStrengthLevel.Strong,
        _ => PasswordStrengthLevel.VeryStrong
    };

    /// <summary>Label FR à afficher dans l'UI.</summary>
    public static string ToLabel(this PasswordStrengthLevel level, Language lang = Language.French)
        => (level, lang) switch
        {
            (PasswordStrengthLevel.Empty, Language.French) => "Vide",
            (PasswordStrengthLevel.VeryWeak, Language.French) => "Très faible",
            (PasswordStrengthLevel.Weak, Language.French) => "Faible",
            (PasswordStrengthLevel.Medium, Language.French) => "Moyen",
            (PasswordStrengthLevel.Strong, Language.French) => "Fort",
            (PasswordStrengthLevel.VeryStrong, Language.French) => "Très fort",

            (PasswordStrengthLevel.Empty, Language.English) => "Empty",
            (PasswordStrengthLevel.VeryWeak, Language.English) => "Very weak",
            (PasswordStrengthLevel.Weak, Language.English) => "Weak",
            (PasswordStrengthLevel.Medium, Language.English) => "Medium",
            (PasswordStrengthLevel.Strong, Language.English) => "Strong",
            (PasswordStrengthLevel.VeryStrong, Language.English) => "Very strong",

            (PasswordStrengthLevel.Empty, Language.Arabic) => "فارغ",
            (PasswordStrengthLevel.VeryWeak, Language.Arabic) => "ضعيف جداً",
            (PasswordStrengthLevel.Weak, Language.Arabic) => "ضعيف",
            (PasswordStrengthLevel.Medium, Language.Arabic) => "متوسط",
            (PasswordStrengthLevel.Strong, Language.Arabic) => "قوي",
            (PasswordStrengthLevel.VeryStrong, Language.Arabic) => "قوي جداً",
            _ => level.ToString()
        };

    /// <summary>Couleur hex associée au niveau pour l'UI.</summary>
    public static string ToHexColor(this PasswordStrengthLevel level) => level switch
    {
        PasswordStrengthLevel.Empty => "#606080",
        PasswordStrengthLevel.VeryWeak => "#EF4444",
        PasswordStrengthLevel.Weak => "#F97316",
        PasswordStrengthLevel.Medium => "#F59E0B",
        PasswordStrengthLevel.Strong => "#84CC16",
        PasswordStrengthLevel.VeryStrong => "#22C55E",
        _ => "#606080"
    };

    /// <summary>Icône/emoji associée pour l'affichage rapide.</summary>
    public static string ToIcon(this PasswordStrengthLevel level) => level switch
    {
        PasswordStrengthLevel.Empty => "⚫",
        PasswordStrengthLevel.VeryWeak => "🔴",
        PasswordStrengthLevel.Weak => "🟠",
        PasswordStrengthLevel.Medium => "🟡",
        PasswordStrengthLevel.Strong => "🟢",
        PasswordStrengthLevel.VeryStrong => "🟢",
        _ => "⚫"
    };

    /// <summary>Retourne true si le niveau est considéré comme acceptable (≥ Fort).</summary>
    public static bool IsAcceptable(this PasswordStrengthLevel level) => level >= PasswordStrengthLevel.Strong;
}