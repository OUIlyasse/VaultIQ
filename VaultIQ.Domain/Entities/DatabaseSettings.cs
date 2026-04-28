namespace VaultIQ.Domain.Entities;

/// <summary>Paramètres de configuration d'une base de données VaultIQ.</summary>
public class DatabaseSettings
{
    public bool EnablePin { get; set; } = false;
    public string PinHash { get; set; } = string.Empty;
    public int AutoLockMinutes { get; set; } = 5;
    public bool EnableCompression { get; set; } = true;
    public int MaxHistoryItems { get; set; } = 200;
    public string PreferredLanguage { get; set; } = "fr";

    public DatabaseSettings Clone() => (DatabaseSettings)MemberwiseClone();
}