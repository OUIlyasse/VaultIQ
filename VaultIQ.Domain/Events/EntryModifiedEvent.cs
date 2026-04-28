namespace VaultIQ.Domain.Events;

/// <summary>
/// Événement domaine émis quand une entrée est créée, modifiée ou supprimée.
/// Utilisé via MediatR INotification dans les Handlers Application.
/// </summary>
public sealed record EntryModifiedEvent(
    Guid EntryId,
    Guid GroupId,
    string EntryTitle,
    string Action,       // "ADD" | "EDIT" | "DELETE" | "DUPLICATE" | "MOVE"
    DateTime OccurredAt) : IDomainEvent
{
    public EntryModifiedEvent(Guid entryId, Guid groupId, string title, string action)
        : this(entryId, groupId, title, action, DateTime.UtcNow) { }
}