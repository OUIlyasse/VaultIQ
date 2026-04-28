namespace VaultIQ.Domain.Events;

/// <summary>Émis quand la base de données est verrouillée (auto-lock ou manuel).</summary>
public sealed record DatabaseLockedEvent(
    Guid DatabaseId,
    string Reason,       // "AUTO" | "MANUAL" | "TIMEOUT"
    DateTime OccurredAt) : IDomainEvent
{
    public DatabaseLockedEvent(Guid databaseId, string reason)
        : this(databaseId, reason, DateTime.UtcNow) { }
}