namespace VaultIQ.Shared.Constants;

/// <summary>
/// Constantes relatives au format de fichier .viq et à son écosystème d'extensions.
/// </summary>
public static class FileConstants
{
    // Extensions
    public const string ExtDatabase = ".viq";

    public const string ExtBackup = ".viqbak";
    public const string ExtExport = ".viqx";
    public const string ExtLog = ".viqlog";
    public const string ExtKeyFile = ".viqkey";
    public const string ExtTemplate = ".viqtpl";
    public const string ExtLegacy = ".pmdb";

    // Magic bytes — "VAULTIQ\x05" (8 octets)
    public const string MagicString = "VAULTIQ\x05";

    public static readonly byte[] MagicBytes = "VAULTIQ\x05"u8.ToArray();

    // Version du format
    public const ushort FormatVersion = 0x0500;

    public const string FormatVersionString = "5.0";

    // Flags uint32 (bit field)
    public const uint FlagCompressed = 0x0001; // GZip avant chiffrement

    public const uint FlagHasRecovery = 0x0002; // Profil de récupération présent
    public const uint FlagKeyFile = 0x0004; // Fichier clé secondaire requis

    // MIME et Windows
    public const string MimeType = "application/x-vaultiq";

    public const string WindowsProgId = "VaultIQ.Database.5";
    public const string FriendlyName = "Base de données VaultIQ";

    // Filtres dialogues WPF
    public const string FileDialogFilter =
        "Base de données VaultIQ (*.viq)|*.viq" +
        "|Sauvegarde VaultIQ (*.viqbak)|*.viqbak" +
        "|Export VaultIQ (*.viqx)|*.viqx" +
        "|Tous les fichiers (*.*)|*.*";

    public const string FileDialogFilterMain =
        "Base de données VaultIQ (*.viq)|*.viq";

    // Offsets binaires dans le fichier .viq
    public const int OffsetMagic = 0;   // 8 o — magic bytes

    public const int OffsetVersion = 8;   // 2 o — uint16 version
    public const int OffsetFlags = 10;  // 4 o — uint32 flags
    public const int OffsetHeaderSize = 14;  // 4 o — uint32 header JSON size
    public const int OffsetHeaderStart = 18;  // variable — début header JSON

    // Noms de dossiers
    public const string AppFolderName = "VaultIQ";

    public const string BackupFolderName = "Backups";
    public const string RecentFileName = "recent.dat";
    public const string SettingsFileName = "settings.json";
}