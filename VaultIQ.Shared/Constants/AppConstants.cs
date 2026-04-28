namespace VaultIQ.Shared.Constants;

/// <summary>
/// Constantes générales de l'application : chemins, timeouts, limites.
/// </summary>
public static class AppConstants
{
    // ── Application ───────────────────────────────────────────────────────
    public const string AppName = "VaultIQ";

    public const string AppVersion = "5.0.0";
    public const string AppFullName = "VaultIQ — Password Manager";
    public const string AppCompany = "VaultIQ";
    public const string AppCopyright = "Copyright © 2025 VaultIQ Contributors";
    public const string AppMotto = "Your vault. Your rules. Intelligent security.";

    // ── Auto-lock ─────────────────────────────────────────────────────────
    /// <summary>Durée d'inactivité par défaut avant verrouillage automatique (minutes).</summary>
    public const int DefaultAutoLockMinutes = 5;

    /// <summary>Durée minimale configurable pour l'auto-lock (minutes).</summary>
    public const int MinAutoLockMinutes = 1;

    /// <summary>Durée maximale configurable pour l'auto-lock (minutes).</summary>
    public const int MaxAutoLockMinutes = 120;

    // ── Historique ────────────────────────────────────────────────────────
    /// <summary>Nombre maximum d'entrées conservées dans l'historique des actions.</summary>
    public const int MaxHistoryItems = 200;

    /// <summary>Nombre de fichiers récents conservés dans la liste.</summary>
    public const int MaxRecentFiles = 15;

    // ── Sauvegarde automatique ────────────────────────────────────────────
    /// <summary>Intervalle par défaut entre deux sauvegardes automatiques (minutes).</summary>
    public const int DefaultBackupIntervalMinutes = 30;

    /// <summary>Intervalle minimum (minutes).</summary>
    public const int MinBackupIntervalMinutes = 5;

    /// <summary>Intervalle maximum (minutes).</summary>
    public const int MaxBackupIntervalMinutes = 1440; // 24h

    /// <summary>Nombre max de sauvegardes automatiques conservées (rotation).</summary>
    public const int DefaultMaxAutoBackups = 10;

    /// <summary>Nombre max de sauvegardes manuelles conservées.</summary>
    public const int DefaultMaxManualBackups = 20;

    // ── Alerte d'expiration ───────────────────────────────────────────────
    /// <summary>Nombre de jours avant expiration à partir duquel l'alerte est affichée.</summary>
    public const int ExpiryWarningDays = 7;

    /// <summary>Intervalle de vérification des expirations (minutes).</summary>
    public const int ExpiryCheckIntervalMin = 60;

    // ── QR Code ───────────────────────────────────────────────────────────
    /// <summary>Durée d'affichage du QR Code avant destruction automatique (secondes).</summary>
    public const int QrCodeDestroySeconds = 30;

    // ── Groupes ───────────────────────────────────────────────────────────
    /// <summary>Icône par défaut pour un nouveau groupe.</summary>
    public const string DefaultGroupIcon = "🔐";

    /// <summary>Nom par défaut du premier groupe créé dans une nouvelle base.</summary>
    public const string DefaultGroupName = "Général";

    // ── Notifications ─────────────────────────────────────────────────────
    /// <summary>Durée d'affichage des balloon tips en millisecondes.</summary>
    public const int BalloonTipDurationMs = 3000;

    // ── URLs ──────────────────────────────────────────────────────────────
    public const string ProjectUrl = "https://github.com/vaultiq/vaultiq";

    public const string IssuesUrl = "https://github.com/vaultiq/vaultiq/issues";
    public const string DocsUrl = "https://docs.vaultiq.app";
}