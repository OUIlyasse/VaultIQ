namespace VaultIQ.Domain.Entities;

/// <summary>
/// Groupe logique d'entrées de mots de passe dans VaultIQ.
/// Contient une liste ordonnée d'entrées. Un groupe ne peut pas être vide.
/// </summary>
public class PasswordGroup
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Icon { get; private set; } = "🔐";
    public DateTime CreatedAt { get; private set; }

    private readonly List<PasswordEntry> _entries = [];
    public IReadOnlyList<PasswordEntry> Entries => _entries.AsReadOnly();

    protected PasswordGroup()
    { }

    // ── Factory ───────────────────────────────────────────────────────────
    public static PasswordGroup Create(
        string name,
        string description = "",
        string icon = "🔐")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        return new PasswordGroup
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            Icon = string.IsNullOrWhiteSpace(icon) ? "🔐" : icon.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    // ── Mutations ─────────────────────────────────────────────────────────
    public void Rename(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));
        Name = newName.Trim();
    }

    public void Update(string name, string description, string icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name.Trim();
        Description = description.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? "🔐" : icon.Trim();
    }

    // ── Entry management ──────────────────────────────────────────────────
    public void AddEntry(PasswordEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.GroupId != Id)
            throw new InvalidOperationException(
                $"Entry GroupId {entry.GroupId} does not match group {Id}.");
        if (_entries.Any(e => e.Id == entry.Id))
            throw new InvalidOperationException(
                $"Entry {entry.Id} already exists in this group.");
        _entries.Add(entry);
    }

    public bool RemoveEntry(Guid entryId)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        return entry is not null && _entries.Remove(entry);
    }

    public PasswordEntry? FindEntry(Guid entryId) =>
        _entries.FirstOrDefault(e => e.Id == entryId);

    // ── Clone ─────────────────────────────────────────────────────────────
    /// <summary>Duplique le groupe et toutes ses entrées sous un nouveau nom.</summary>
    public PasswordGroup Clone(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));
        var clone = Create(newName, Description, Icon);
        foreach (var entry in _entries)
            clone._entries.Add(entry.Clone(clone.Id));
        return clone;
    }

    // ── Computed ──────────────────────────────────────────────────────────
    public int EntryCount => _entries.Count;

    public int FavoriteCount => _entries.Count(e => e.IsFavorite);

    public int WeakCount => _entries.Count(e =>
        e.StrengthScore is > 0 and < 40);

    public int ExpiringCount => _entries.Count(e => e.IsExpiringSoon);

    public override string ToString() =>
        $"[{Id:N}] {Icon} {Name} ({EntryCount} entries)";
}