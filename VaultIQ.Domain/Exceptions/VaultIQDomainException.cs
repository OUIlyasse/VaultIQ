namespace VaultIQ.Domain.Exceptions;

/// <summary>
/// Exception de base pour toutes les erreurs du domaine VaultIQ.
/// Ne pas utiliser directement — utiliser les sous-classes spécifiques.
/// </summary>
public abstract class VaultIQDomainException : Exception
{
    protected VaultIQDomainException(string message) : base(message)
    {
    }

    protected VaultIQDomainException(string message, Exception inner) : base(message, inner)
    {
    }
}