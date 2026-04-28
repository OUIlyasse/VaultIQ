namespace VaultIQ.Shared.DTOs;

/// <summary>
/// Résumé public d'une base de données .viq lisible SANS mot de passe.
/// Correspond au header JSON stocké en clair dans le fichier.
/// Affiché dans la fenêtre de connexion avant toute saisie.
/// </summary>
public sealed record DatabaseInfoDto
{
    public string AppName { get; init; } = "VaultIQ";
    public string AppVersion { get; init; } = "5.0";
    public string DatabaseName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ModifiedAt { get; init; }
    public int EntryCount { get; init; }
    public int GroupCount { get; init; }
    public string KdfAlgorithm { get; init; } = "PBKDF2-SHA256";
    public int KdfIterations { get; init; } = 150_000;
    public string EncAlgorithm { get; init; } = "AES-256-CBC";
    public string Checksum { get; init; } = string.Empty;
    public bool HasRecovery { get; init; }
    public string FormatVersion { get; init; } = "5.0";
    public string Platform { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }

    public string FileSizeFormatted => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} o",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} Ko",
        _ => $"{FileSizeBytes / (1024.0 * 1024):F2} Mo"
    };
}