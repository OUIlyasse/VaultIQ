using System.Text.RegularExpressions;

namespace VaultIQ.Domain.ValueObjects;

/// <summary>
/// Value Object pour un numéro de téléphone de récupération.
/// Normalisé au format E.164, masqué à l'affichage.
/// La valeur en clair n'est jamais exposée après création — uniquement le masqué.
/// </summary>
public sealed record PhoneNumber
{
    /// <summary>Numéro normalisé E.164 (ex: "+33612345678").</summary>
    public string Normalized { get; }

    /// <summary>Indicatif pays (ex: "+33").</summary>
    public string CountryCode { get; }

    /// <summary>Numéro masqué pour l'affichage (ex: "+33 06** **** 78").</summary>
    public string Masked { get; }

    private PhoneNumber(string normalized, string countryCode, string masked)
    {
        Normalized = normalized;
        CountryCode = countryCode;
        Masked = masked;
    }

    /// <summary>Crée un <see cref="PhoneNumber"/> validé et normalisé.</summary>
    public static PhoneNumber Create(string raw, string countryCode = "+33")
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Le numéro de téléphone est obligatoire.", nameof(raw));

        string normalized = Normalize(raw, countryCode);

        if (!IsValid(normalized))
            throw new ArgumentException(
                $"Numéro de téléphone invalide : '{raw}'. Format attendu : 0612345678 ou +33612345678.",
                nameof(raw));

        string masked = BuildMasked(normalized, countryCode);
        return new PhoneNumber(normalized, countryCode, masked);
    }

    /// <summary>Tente de créer, retourne null si invalide.</summary>
    public static PhoneNumber? TryCreate(string? raw, string countryCode = "+33")
    {
        try { return string.IsNullOrWhiteSpace(raw) ? null : Create(raw, countryCode); }
        catch { return null; }
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static string Normalize(string phone, string defaultCC)
    {
        string clean = Regex.Replace(phone, @"[\s\.\-\(\)]", "");
        if (clean.StartsWith("00")) clean = "+" + clean[2..];
        if (clean.StartsWith("0") && clean.Length == 10)
            clean = defaultCC + clean[1..];
        return clean;
    }

    private static bool IsValid(string normalized)
        => Regex.IsMatch(normalized, @"^\+\d{8,15}$");

    private static string BuildMasked(string normalized, string cc)
    {
        string digits = Regex.Replace(normalized, @"[^\d]", "");
        if (digits.Length < 6) return "***";
        string start = digits[..2];
        string end = digits[^2..];
        string middle = new('*', digits.Length - 4);
        return $"{cc} {start}{middle}{end}";
    }

    // Masque la valeur lors du ToString()
    public override string ToString() => Masked;
}