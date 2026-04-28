namespace VaultIQ.Domain.Events;

/// <summary>
/// Marqueur commun pour tous les événements domaine VaultIQ.
/// Les événements sont dispatchés via MediatR INotification dans la couche Application.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}