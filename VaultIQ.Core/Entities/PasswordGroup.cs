namespace VaultIQ.Core.Entities;

/// <summary>Groupe logique d'entrées de mots de passe.</summary>
public class PasswordGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string IconPath { get; set; } = "";
    public List<PasswordEntry> Entries { get; set; } = [];
    public string Color { get; set; } = "#378ADD";
    public int EntryCount => Entries.Count;
}