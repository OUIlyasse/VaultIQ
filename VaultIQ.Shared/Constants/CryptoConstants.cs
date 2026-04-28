namespace VaultIQ.Shared.Constants;

/// <summary>
/// Constantes cryptographiques utilisées dans toute la solution.
/// Centralisées ici pour éviter la duplication entre Infrastructure et Domain.
/// </summary>
public static class CryptoConstants
{
    // ── AES-256 ───────────────────────────────────────────────────────────
    /// <summary>Longueur de la clé AES en bits.</summary>
    public const int AesKeyBits = 256;

    /// <summary>Longueur de la clé AES en octets (32).</summary>
    public const int AesKeyBytes = AesKeyBits / 8;

    /// <summary>Longueur du vecteur d'initialisation AES-CBC en octets (16).</summary>
    public const int AesIvBytes = 16;

    // ── PBKDF2 — Clé principale ───────────────────────────────────────────
    /// <summary>
    /// Nombre d'itérations PBKDF2-SHA256 pour dériver la clé AES principale.
    /// 150 000 itérations ≈ 200 ms sur GPU RTX 4090 — résistance brute-force.
    /// </summary>
    public const int KdfIterations = 150_000;

    /// <summary>
    /// Nombre d'itérations PBKDF2 pour dériver la clé HMAC séparée.
    /// Utilise un sous-ensemble différent du même sel.
    /// </summary>
    public const int HmacIterations = 75_000;

    /// <summary>Taille du sel KDF en octets (256 bits).</summary>
    public const int SaltBytes = 32;

    // ── PBKDF2 — Récupération de compte ──────────────────────────────────
    /// <summary>Itérations pour le hash des données de récupération (tél/email/PIN/QA).</summary>
    public const int RecoveryIterations = 100_000;

    /// <summary>Taille du sel dédié à la récupération (256 bits).</summary>
    public const int RecoverySaltBytes = 32;

    // ── HMAC-SHA256 ───────────────────────────────────────────────────────
    /// <summary>Taille d'un hash HMAC-SHA256 en octets (32).</summary>
    public const int HmacBytes = 32;

    // ── SHA-256 checksum ─────────────────────────────────────────────────
    /// <summary>Taille du checksum SHA-256 en octets (32).</summary>
    public const int ChecksumBytes = 32;

    // ── Génération de mots de passe ───────────────────────────────────────
    /// <summary>Longueur minimale d'un mot de passe généré.</summary>
    public const int MinPasswordLength = 4;

    /// <summary>Longueur maximale d'un mot de passe généré.</summary>
    public const int MaxPasswordLength = 128;

    /// <summary>Longueur par défaut du générateur.</summary>
    public const int DefaultPasswordLength = 20;

    // ── PIN ───────────────────────────────────────────────────────────────
    /// <summary>Longueur minimale d'un code PIN (accès ou récupération).</summary>
    public const int MinPinLength = 4;

    /// <summary>Longueur maximale d'un code PIN.</summary>
    public const int MaxPinLength = 8;

    /// <summary>Nombre maximum de tentatives PIN avant blocage.</summary>
    public const int MaxPinAttempts = 5;

    /// <summary>Durée de blocage PIN en minutes après dépassement du maximum.</summary>
    public const int PinLockoutMinutes = 30;

    // ── Presse-papier ─────────────────────────────────────────────────────
    /// <summary>
    /// Délai en secondes avant effacement automatique du presse-papier
    /// après copie d'un mot de passe.
    /// </summary>
    public const int ClipboardClearSeconds = 15;

    // ── Clé maître ───────────────────────────────────────────────────────
    /// <summary>Longueur minimale du mot de passe principal.</summary>
    public const int MinMasterKeyLength = 4;

    /// <summary>Longueur recommandée du mot de passe principal.</summary>
    public const int MasterKeyRecommendedLength = 12;
}