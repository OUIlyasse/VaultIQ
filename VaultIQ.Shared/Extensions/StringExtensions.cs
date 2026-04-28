using System.Text.RegularExpressions;

namespace VaultIQ.Shared.Extensions;

/// <summary>
/// Extensions sur string utilisées dans toute la solution VaultIQ.
/// </summary>
public static class StringExtensions
{
    // ── Email masking ─────────────────────────────────────────────────────
    /// <summary>
    /// Masque un email pour l'affichage : "jean.dupont@gmail.com" → "j***@gmail.com"
    /// </summary>
    public static string MaskEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "***";

        var parts = email.Split('@');
        if (parts.Length != 2) return "***@***.***";

        string name = parts[0];
        string domain = parts[1];

        if (name.Length <= 1) return $"{name}@{domain}";

        int visibleChars = Math.Max(1, Math.Min(2, name.Length / 3));
        string masked = name[..visibleChars] + new string('*', Math.Min(name.Length - visibleChars, 4));
        return $"{masked}@{domain}";
    }

    // ── Phone masking ─────────────────────────────────────────────────────
    /// <summary>
    /// Masque un numéro de téléphone : "+33612345678" → "+33 6** **** 78"
    /// </summary>
    public static string MaskPhone(this string normalizedPhone)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhone)) return "***";

        string digitsOnly = Regex.Replace(normalizedPhone, @"[^\d]", "");
        if (digitsOnly.Length < 6) return "***";

        string visible_start = digitsOnly[..2];
        string visible_end = digitsOnly[^2..];
        string middle = new('*', digitsOnly.Length - 4);

        string countryCode = normalizedPhone.StartsWith('+')
            ? "+" + Regex.Match(normalizedPhone, @"^\+(\d{1,3})").Groups[1].Value + " "
            : "";

        return $"{countryCode}{visible_start}{middle}{visible_end}";
    }

    // ── Truncate ──────────────────────────────────────────────────────────
    /// <summary>
    /// Tronque une chaîne à la longueur maximale avec ellipsis si nécessaire.
    /// </summary>
    public static string Truncate(this string value, int maxLength, string ellipsis = "…")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
        return string.Concat(value.AsSpan(0, maxLength - ellipsis.Length), ellipsis);
    }

    // ── Null / empty guards ───────────────────────────────────────────────
    /// <summary>Retourne la valeur si non nulle/vide, sinon le fallback.</summary>
    public static string OrDefault(this string? value, string fallback = "") =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    // ── Phone normalisation ───────────────────────────────────────────────
    /// <summary>
    /// Normalise un numéro de téléphone au format E.164.
    /// "0612345678" (France) → "+33612345678"
    /// </summary>
    public static string NormalizePhone(this string phone, string defaultCountryCode = "+33")
    {
        string digits = Regex.Replace(phone, @"[^\d+]", "");

        if (digits.StartsWith('+')) return digits;

        // Numéro local français 0X → +33X
        if (digits.StartsWith('0') && digits.Length == 10)
            return defaultCountryCode + digits[1..];

        return defaultCountryCode + digits;
    }

    /// <summary>Vérifie qu'un numéro normalisé E.164 est valide (8-15 chiffres après +CC).</summary>
    public static bool IsValidPhone(this string normalizedPhone) =>
        Regex.IsMatch(normalizedPhone, @"^\+\d{8,15}$");

    /// <summary>Vérifie qu'une adresse email est syntaxiquement valide.</summary>
    public static bool IsValidEmail(this string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");

    // ── Security answer normalisation ─────────────────────────────────────
    /// <summary>
    /// Normalise une réponse à une question secrète pour le hashage :
    /// lowercase + trim + suppression des espaces multiples.
    /// </summary>
    public static string NormalizeSecurityAnswer(this string answer) =>
        Regex.Replace(answer.ToLowerInvariant().Trim(), @"\s+", " ");

    // ── Misc ──────────────────────────────────────────────────────────────
    /// <summary>Convertit une chaîne en titre (première lettre en majuscule).</summary>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }
}