using VaultIQ.Shared.Enums;
using VaultIQ.Shared.Extensions;

namespace VaultIQ.Domain.Entities;

/// <summary>
/// Entité principale représentant une entrée de mot de passe dans VaultIQ.
/// Chaque entrée appartient à exactement un <see cref="PasswordGroup"/>.
/// </summary>
public class PasswordEntry
{
    // ── Identité ──────────────────────────────────────────────────────────
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid GroupId { get; private set; }

    // ── Données utilisateur ───────────────────────────────────────────────
    public string Title { get; private set; } = string.Empty;

    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public string Tags { get; private set; } = string.Empty;

    // ── Flags ─────────────────────────────────────────────────────────────
    public bool IsFavorite { get; private set; }

    // ── Dates ─────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; private set; }

    public DateTime ModifiedAt { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    // ── Statistiques ──────────────────────────────────────────────────────
    public int UsageCount { get; private set; }

    public int StrengthScore { get; private set; }

    // ── Constructeur protégé (désérialisation JSON) ───────────────────────
    protected PasswordEntry()
    { }

    // ── Factory method ────────────────────────────────────────────────────
    /// <summary>Crée une nouvelle entrée. Le titre et le groupId sont obligatoires.</summary>
    public static PasswordEntry Create(
        Guid groupId,
        string title,
        string username = "",
        string password = "",
        string url = "",
        string notes = "",
        string tags = "",
        int strength = 0,
        DateTime? expiry = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        if (groupId == Guid.Empty)
            throw new ArgumentException("GroupId cannot be empty.", nameof(groupId));

        return new PasswordEntry
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Title = title.Trim(),
            Username = username.Trim(),
            Password = password,
            Url = NormalizeUrl(url),
            Notes = notes,
            Tags = tags.Trim(),
            StrengthScore = Math.Clamp(strength, 0, 100),
            ExpiryDate = expiry?.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
        };
    }

    // ── Update ────────────────────────────────────────────────────────────
    public void Update(
        string title,
        string username,
        string password,
        string url,
        string notes,
        string tags,
        int strengthScore,
        DateTime? expiryDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        Title = title.Trim();
        Username = username.Trim();
        Password = password;
        Url = NormalizeUrl(url);
        Notes = notes;
        Tags = tags.Trim();
        StrengthScore = Math.Clamp(strengthScore, 0, 100);
        ExpiryDate = expiryDate?.ToUniversalTime();
        ModifiedAt = DateTime.UtcNow;
    }

    public void MoveToGroup(Guid newGroupId)
    {
        if (newGroupId == Guid.Empty)
            throw new ArgumentException("GroupId cannot be empty.");
        GroupId = newGroupId;
        ModifiedAt = DateTime.UtcNow;
    }

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetFavorite(bool value)
    {
        if (IsFavorite == value) return;
        IsFavorite = value;
        ModifiedAt = DateTime.UtcNow;
    }

    public void IncrementUsage() => UsageCount++;

    // ── Computed ──────────────────────────────────────────────────────────
    public PasswordStrengthLevel StrengthLevel =>
        PasswordStrengthLevelExtensions.FromScore(StrengthScore);

    public bool IsExpired => ExpiryDate.IsExpired();

    public bool IsExpiringSoon => ExpiryDate.IsExpiringSoon();

    public string MaskedPassword => new('•', Math.Min(Password.Length, 12));

    // ── Clone ─────────────────────────────────────────────────────────────
    /// <summary>Clone l'entrée dans un nouveau groupe avec un titre optionnel.</summary>
    public PasswordEntry Clone(Guid targetGroupId, string? newTitle = null)
    {
        if (targetGroupId == Guid.Empty)
            throw new ArgumentException("GroupId cannot be empty.");
        return new PasswordEntry
        {
            Id = Guid.NewGuid(),
            GroupId = targetGroupId,
            Title = (newTitle ?? Title + " (copie)").Trim(),
            Username = Username,
            Password = Password,
            Url = Url,
            Notes = Notes,
            Tags = Tags,
            StrengthScore = StrengthScore,
            ExpiryDate = ExpiryDate,
            IsFavorite = false,
            UsageCount = 0,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        url = url.Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
            url.Contains('.'))
            return "https://" + url;
        return url;
    }

    public override string ToString() => $"[{Id:N}] {Title} ({Username})";
}