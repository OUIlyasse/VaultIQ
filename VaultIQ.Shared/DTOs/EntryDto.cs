using VaultIQ.Shared.Enums;
using VaultIQ.Shared.Extensions;

namespace VaultIQ.Shared.DTOs;

/// <summary>
/// DTO sérialisable représentant une entrée de mot de passe.
/// Utilisé pour le transfert de données entre couches (Application ↔ WPF).
/// </summary>
public sealed record EntryDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid GroupId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public string Tags { get; init; } = string.Empty;
    public bool IsFavorite { get; init; } = false;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; init; }
    public int StrengthScore { get; init; } = 0;
    public int UsageCount { get; init; } = 0;

    // Propriétés calculées (non sérialisées)
    public PasswordStrengthLevel StrengthLevel =>
        PasswordStrengthLevelExtensions.FromScore(StrengthScore);

    public bool IsExpired => ExpiryDate.IsExpired();

    public bool IsExpiringSoon =>
        ExpiryDate.IsExpiringSoon();

    public string MaskedPassword => new('•', Math.Min(Password.Length, 12));
}