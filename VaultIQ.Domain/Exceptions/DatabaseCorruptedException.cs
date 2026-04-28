namespace VaultIQ.Domain.Exceptions;

/// <summary>
/// Levée quand l'intégrité HMAC du fichier .viq échoue,
/// signalant une corruption ou une modification non autorisée.
/// </summary>
public sealed class DatabaseCorruptedException : VaultIQDomainException
{
    public DatabaseCorruptedException(string message) : base(message)
    {
    }

    public DatabaseCorruptedException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>Erreur standard lorsque la vérification HMAC-SHA256 échoue.</summary>
    public static DatabaseCorruptedException IntegrityCheckFailed(string detail = "") =>
        new($"Vérification d'intégrité HMAC-SHA256 échouée. " +
            "Le fichier a peut-être été modifié ou corrompu." +
            (string.IsNullOrEmpty(detail) ? "" : $" Détail : {detail}"));

    /// <summary>Erreur quand le format du fichier n'est pas reconnu (magic bytes invalides).</summary>
    public static DatabaseCorruptedException InvalidFormat() =>
        new("Le fichier n'est pas une base VaultIQ valide.");

    /// <summary>Erreur quand la version du fichier est trop récente pour cette version de VaultIQ.</summary>
    public static DatabaseCorruptedException UnsupportedVersion(ushort version) =>
        new($"Version de fichier non supportée (0x{version:X4}). " +
            "Mettez à jour VaultIQ vers une version récente.");
}