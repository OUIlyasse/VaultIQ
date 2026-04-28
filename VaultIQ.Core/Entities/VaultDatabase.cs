namespace VaultIQ.Core.Entities;

/// <summary>Base de données VaultIQ complète — chargée en mémoire après ouverture.</summary>
public class VaultDatabase
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public List<PasswordGroup> Groups { get; set; } = [];

    // Propriétés calculées
    public int TotalEntries => Groups.Sum(g => g.Entries.Count);

    public int TotalFavorites => Groups.SelectMany(g => g.Entries).Count(e => e.IsFavorite);

    public IEnumerable<PasswordEntry> AllEntries =>
        Groups.SelectMany(g => g.Entries);

    public IEnumerable<PasswordEntry> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return AllEntries;
        return AllEntries.Where(e =>
            e.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Url.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Notes.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public PasswordGroup? FindGroupOf(Guid entryId) =>
        Groups.FirstOrDefault(g => g.Entries.Any(e => e.Id == entryId));

    public void Touch() => ModifiedAt = DateTime.UtcNow;
}