namespace VaultIQ.Domain.Exceptions;

/// <summary>
/// Levée quand le mot de passe principal est invalide (vide, trop court,
/// ou incorrect lors de l'ouverture d'une base).
/// </summary>
public sealed class InvalidMasterKeyException : VaultIQDomainException
{
    public InvalidMasterKeyException(string message) : base(message)
    {
    }

    public InvalidMasterKeyException(string message, Exception inner) : base(message, inner)
    {
    }

    /// <summary>Erreur standard lors d'un mot de passe incorrect à l'ouverture.</summary>
    public static InvalidMasterKeyException WrongPassword() =>
        new("Mot de passe principal incorrect ou base de données corrompue.");
}