namespace VaultIQ.Domain.Entities;

/// <summary>Entrée du journal d'audit.</summary>
public class HistoryEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string GroupName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;  // ADD / EDIT / DELETE / DUPLICATE
    public DateTime Date { get; private set; }

    protected HistoryEntry()
    { }

    public static HistoryEntry Create(string groupName, string description, string action) =>
        new()
        {
            Id = Guid.NewGuid(),
            GroupName = groupName,
            Description = description,
            Action = action.ToUpperInvariant(),
            Date = DateTime.UtcNow,
        };

    public string ActionIcon => Action switch
    {
        "ADD" => "➕",
        "EDIT" => "✏️",
        "DELETE" => "🗑️",
        "DUPLICATE" => "📋",
        "LOCK" => "🔒",
        "UNLOCK" => "🔓",
        _ => "•"
    };
}