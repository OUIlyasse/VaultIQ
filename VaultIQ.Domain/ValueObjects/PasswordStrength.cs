using System.Text.RegularExpressions;
using VaultIQ.Shared.Enums;

namespace VaultIQ.Domain.ValueObjects;

/// <summary>
/// Value Object représentant la force d'un mot de passe.
/// Le score est calculé de manière déterministe depuis la chaîne en clair.
/// </summary>
public sealed record PasswordStrength
{
    /// <summary>Score brut de 0 à 100.</summary>
    public int Score { get; }

    /// <summary>Niveau catégorisé.</summary>
    public PasswordStrengthLevel Level { get; }

    /// <summary>Entropie estimée en bits.</summary>
    public double EntropyBits { get; }

    private PasswordStrength(int score, PasswordStrengthLevel level, double entropy)
    {
        Score = score;
        Level = level;
        EntropyBits = Math.Round(entropy, 1);
    }

    /// <summary>Calcule la force d'un mot de passe.</summary>
    public static PasswordStrength Calculate(string? password)
    {
        if (string.IsNullOrEmpty(password)) return new(0, PasswordStrengthLevel.Empty, 0);

        int score = 0;
        int len = password.Length;

        // Longueur (0–40 pts)
        score += len switch { >= 20 => 40, >= 16 => 35, >= 12 => 28, >= 8 => 18, >= 6 => 8, _ => 2 };

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSymbol = password.Any(c => !char.IsLetterOrDigit(c));

        int charsets = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSymbol ? 1 : 0);

        // Diversité (0–40 pts)
        score += charsets switch { 4 => 40, 3 => 30, 2 => 18, 1 => 5, _ => 0 };

        // Bonus motifs (0–20 pts)
        if (!Regex.IsMatch(password, @"(.)\1{2,}")) score += 8;  // pas de répétitions
        if (!Regex.IsMatch(password, @"(012|123|234|345|456|567|678|789|890|abc|bcd|cde|qwe|wer)", RegexOptions.IgnoreCase)) score += 7;
        if (len >= 8 && charsets >= 3) score += 5;

        score = Math.Clamp(score, 0, 100);

        // Entropie : log2(pool^len)
        int pool = (hasLower ? 26 : 0) + (hasUpper ? 26 : 0) + (hasDigit ? 10 : 0) + (hasSymbol ? 32 : 0);
        if (pool == 0) pool = 26;
        double entropy = len * Math.Log2(pool);

        return new(score, PasswordStrengthLevelExtensions.FromScore(score), entropy);
    }

    /// <summary>Retourne le label localisé.</summary>
    public string GetLabel(Language lang = Language.French) => Level.ToLabel(lang);

    /// <summary>Couleur hexadécimale associée.</summary>
    public string HexColor => Level.ToHexColor();

    /// <summary>Vrai si le score est ≥ au seuil "Fort".</summary>
    public bool IsAcceptable => Level.IsAcceptable();

    public override string ToString() => $"[Strength {Score}/100 — {Level}]";
}