using VaultIQ.Shared.Constants;

namespace VaultIQ.Domain.ValueObjects;

/// <summary>
/// Value Object pour un code PIN de 4 à 8 chiffres.
/// Utilisé pour le PIN d'accès ET le PIN de récupération (types distincts).
/// </summary>
public sealed record PinCode
{
    /// <summary>Valeur en clair (en mémoire uniquement).</summary>
    public string Value { get; }

    /// <summary>Nombre de chiffres.</summary>
    public int Length => Value.Length;

    private PinCode(string value) => Value = value;

    /// <summary>Crée un <see cref="PinCode"/> validé.</summary>
    /// <exception cref="ArgumentException">Si le PIN n'est pas valide.</exception>
    public static PinCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Le code PIN ne peut pas être vide.", nameof(value));

        if (!value.All(char.IsDigit))
            throw new ArgumentException("Le code PIN doit contenir uniquement des chiffres.", nameof(value));

        if (value.Length < CryptoConstants.MinPinLength || value.Length > CryptoConstants.MaxPinLength)
            throw new ArgumentException(
                $"Le code PIN doit contenir entre {CryptoConstants.MinPinLength} et {CryptoConstants.MaxPinLength} chiffres.",
                nameof(value));

        return new PinCode(value);
    }

    /// <summary>Tente de créer, retourne null si invalide.</summary>
    public static PinCode? TryCreate(string? value)
    {
        try { return string.IsNullOrWhiteSpace(value) ? null : Create(value); }
        catch { return null; }
    }

    public override string ToString() => new('•', Value.Length);
}