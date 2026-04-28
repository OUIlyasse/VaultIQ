namespace VaultIQ.Shared.Enums;

/// <summary>Type d'une sauvegarde de base de données.</summary>
public enum BackupType
{
    /// <summary>Sauvegarde déclenchée automatiquement par le timer (extension .viqbak).</summary>
    Automatic = 0,

    /// <summary>Sauvegarde déclenchée manuellement par l'utilisateur (extension .viqbak).</summary>
    Manual = 1
}

public static class BackupTypeExtensions
{
    public static string ToLabel(this BackupType t) => t switch
    {
        BackupType.Automatic => "🤖 Automatique",
        BackupType.Manual => "🖐️ Manuel",
        _ => t.ToString()
    };

    /// <summary>Suffixe inséré dans le nom de fichier pour distinguer les types.</summary>
    public static string ToFileSuffix(this BackupType t) => t switch
    {
        BackupType.Automatic => "_auto",
        BackupType.Manual => "_manual",
        _ => "_bak"
    };
}