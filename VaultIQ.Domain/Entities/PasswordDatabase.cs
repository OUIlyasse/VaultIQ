using VaultIQ.Shared.Constants;

namespace VaultIQ.Domain.Entities;

/// <summary>
/// Agrégat racine VaultIQ — représente une base de données complète (.viq).
/// Contient les groupes, les paramètres et le profil de récupération.
/// </summary>
public class PasswordDatabase
{
    // ── Identité de la base ───────────────────────────────────────────────
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ModifiedAt { get; private set; }
    public string Version { get; private set; } = FileConstants.FormatVersionString;

    // ── Paramètres ────────────────────────────────────────────────────────
    public DatabaseSettings Settings { get; private set; } = new();

    public RecoveryProfile Recovery { get; private set; } = new();

    // ── Données ───────────────────────────────────────────────────────────
    private readonly List<PasswordGroup> _groups = [];

    private readonly List<HistoryEntry> _history = [];

    public IReadOnlyList<PasswordGroup> Groups => _groups.AsReadOnly();
    public IReadOnlyList<HistoryEntry> History => _history.AsReadOnly();

    protected PasswordDatabase()
    { }

    // ── Factory ───────────────────────────────────────────────────────────
    public static PasswordDatabase Create(string name, string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        return new PasswordDatabase
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
        };
    }

    // ── Group management ──────────────────────────────────────────────────
    public void AddGroup(PasswordGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (_groups.Any(g => g.Id == group.Id))
            throw new InvalidOperationException($"Group {group.Id} already exists.");
        _groups.Add(group);
        Touch();
    }

    public bool RemoveGroup(Guid groupId)
    {
        var group = _groups.FirstOrDefault(g => g.Id == groupId);
        if (group is null) return false;
        _groups.Remove(group);
        Touch();
        return true;
    }

    public PasswordGroup? FindGroup(Guid groupId) =>
        _groups.FirstOrDefault(g => g.Id == groupId);

    // ── Cross-group entry search ──────────────────────────────────────────
    public IEnumerable<PasswordEntry> AllEntries =>
        _groups.SelectMany(g => g.Entries);

    public IEnumerable<PasswordEntry> Favorites =>
        AllEntries.Where(e => e.IsFavorite);

    public IEnumerable<PasswordEntry> ExpiringEntries =>
        AllEntries.Where(e => e.IsExpiringSoon);

    public IEnumerable<PasswordEntry> ExpiredEntries =>
        AllEntries.Where(e => e.IsExpired);

    public IEnumerable<PasswordEntry> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return AllEntries;
        query = query.ToLowerInvariant().Trim();
        return AllEntries.Where(e =>
            e.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Url.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Notes.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Tags.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    // ── History ───────────────────────────────────────────────────────────
    public void AddHistory(string groupName, string description, string action)
    {
        _history.Insert(0, HistoryEntry.Create(groupName, description, action));
        if (_history.Count > AppConstants.MaxHistoryItems)
            _history.RemoveRange(
                AppConstants.MaxHistoryItems,
                _history.Count - AppConstants.MaxHistoryItems);
    }

    public void ClearHistory() => _history.Clear();

    // ── Settings / Recovery ───────────────────────────────────────────────
    public void UpdateSettings(DatabaseSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Settings = settings;
        Touch();
    }

    public void UpdateRecovery(RecoveryProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        Recovery = profile;
        Touch();
    }

    // ── Meta ──────────────────────────────────────────────────────────────
    public void Rename(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));
        Name = newName.Trim();
        Touch();
    }

    private void Touch() => ModifiedAt = DateTime.UtcNow;

    // ── Statistics ────────────────────────────────────────────────────────
    public int TotalEntries => AllEntries.Count();

    public int TotalFavorites => AllEntries.Count(e => e.IsFavorite);
    public int TotalWeak => AllEntries.Count(e => e.StrengthScore is > 0 and < 40);
    public int TotalExpiring => AllEntries.Count(e => e.IsExpiringSoon);

    public double SecurityScore
    {
        get
        {
            int total = TotalEntries;
            if (total == 0) return 0;
            int strong = AllEntries.Count(e => e.StrengthScore >= 80);
            return Math.Round((double)strong / total * 100, 1);
        }
    }

    public override string ToString() => $"[{Id:N}] {Name} ({TotalEntries} entries)";
}