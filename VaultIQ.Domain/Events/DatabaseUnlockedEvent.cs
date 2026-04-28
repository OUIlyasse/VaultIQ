namespace VaultIQ.Domain.Events;

/// <summary>Émis quand la base de données est déverrouillée avec succès.</summary>
public sealed record DatabaseUnlockedEvent(
    Guid DatabaseId,
    DateTime OccurredAt) : IDomainEvent
{
    public DatabaseUnlockedEvent(Guid databaseId)
        : this(databaseId, DateTime.UtcNow) { }
}