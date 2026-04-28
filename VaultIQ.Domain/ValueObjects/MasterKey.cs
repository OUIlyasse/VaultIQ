using VaultIQ.Shared.Constants;

namespace VaultIQ.Domain.ValueObjects;

/// <summary>
/// Value Object représentant la clé principale de la base de données.
/// Immuable, validée à la construction.
/// La valeur en clair n'est PAS stockée dans la base de données.
/// </summary>
public sealed record MasterKey
{
    /// <summary>Valeur en clair (en mémoire uniquement, jamais sérialisée).</summary>
    public string Value { get; }

    private MasterKey(string value) => Value = value;

    /// <summary>Crée une <see cref="MasterKey"/> validée.</summary>
    /// <exception cref="Exceptions.InvalidMasterKeyException">Si la clé est invalide.</exception>
    public static MasterKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exceptions.InvalidMasterKeyException("La clé principale ne peut pas être vide.");

        if (value.Length < CryptoConstants.MinMasterKeyLength)
            throw new Exceptions.InvalidMasterKeyException(
                $"La clé principale doit contenir au moins {CryptoConstants.MinMasterKeyLength} caractères.");

        return new MasterKey(value);
    }

    /// <summary>Tente de créer une MasterKey, retourne null si invalide.</summary>
    public static MasterKey? TryCreate(string? value)
    {
        try { return string.IsNullOrWhiteSpace(value) ? null : Create(value); }
        catch { return null; }
    }

    /// <summary>Longueur de la clé.</summary>
    public int Length => Value.Length;

    /// <summary>Vrai si la clé est considérée comme forte (≥ 12 chars recommandés).</summary>
    public bool IsStrong => Value.Length >= CryptoConstants.MasterKeyRecommendedLength;

    // Empêche la sérialisation accidentelle
    public override string ToString() => $"[MasterKey length={Value.Length}]";
}