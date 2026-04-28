namespace VaultIQ.Domain.Exceptions;

/// <summary>
/// Levée quand le code PIN est bloqué suite à trop de tentatives échouées.
/// </summary>
public sealed class PinLockedOutException : VaultIQDomainException
{
    /// <summary>Durée de blocage restante en minutes.</summary>
    public int MinutesRemaining { get; }

    public PinLockedOutException(int minutesRemaining)
        : base($"Code PIN bloqué. Réessayez dans {minutesRemaining} minute(s).")
    {
        MinutesRemaining = minutesRemaining;
    }
}