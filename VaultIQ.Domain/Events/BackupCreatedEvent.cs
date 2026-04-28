namespace VaultIQ.Domain.Events;

/// <summary>Émis quand une sauvegarde automatique ou manuelle est créée avec succès.</summary>
public sealed record BackupCreatedEvent(
    Guid DatabaseId,
    string BackupFilePath,
    string BackupType,     // "AUTO" | "MANUAL"
    long SizeBytes,
    DateTime OccurredAt) : IDomainEvent
{
    public BackupCreatedEvent(Guid dbId, string path, string type, long size)
        : this(dbId, path, type, size, DateTime.UtcNow) { }
}