using VaultIQ.Shared.Enums;

namespace VaultIQ.Shared.DTOs;

/// <summary>
/// Informations sur une sauvegarde de base de données.
/// Utilisé dans le gestionnaire de sauvegardes.
/// </summary>
public sealed record BackupInfoDto
{
    public string FilePath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public long SizeBytes { get; init; }
    public BackupType BackupType { get; init; }

    public string SizeFormatted => SizeBytes switch
    {
        < 1024 => $"{SizeBytes} o",
        < 1024 * 1024 => $"{SizeBytes / 1024.0:F1} Ko",
        _ => $"{SizeBytes / (1024.0 * 1024):F2} Mo"
    };

    public string TypeLabel => BackupType.ToLabel();
    public string CreatedAtFormatted => CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
}