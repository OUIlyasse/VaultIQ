namespace VaultIQ.Shared.Enums;

/// <summary>
/// Modèles d'entrées prédéfinis (16 templates) regroupés par catégorie.
/// </summary>
public enum EntryTemplate
{
    // Catégorie : Compte en ligne
    Email = 1,

    SocialMedia = 2,
    Website = 3,
    Gaming = 4,
    Streaming = 5,

    // Catégorie : Finance
    BankCard = 10,

    BankAccount = 11,
    Crypto = 12,

    // Catégorie : Réseau
    WiFi = 20,

    SshFtp = 21,
    Vpn = 22,
    Database = 23,

    // Catégorie : Appareil
    Computer = 30,

    MobileApp = 31,

    // Catégorie : Personnel
    IdDocument = 40,

    // Catégorie : Logiciel
    LicenseKey = 50,

    // Catégorie : Note
    SecureNote = 60
}

public static class EntryTemplateExtensions
{
    /// <summary>Icône emoji associée au modèle.</summary>
    public static string ToIcon(this EntryTemplate t) => t switch
    {
        EntryTemplate.Email => "✉️",
        EntryTemplate.SocialMedia => "👤",
        EntryTemplate.Website => "🌐",
        EntryTemplate.Gaming => "🎮",
        EntryTemplate.Streaming => "🎬",
        EntryTemplate.BankCard => "💳",
        EntryTemplate.BankAccount => "🏦",
        EntryTemplate.Crypto => "₿",
        EntryTemplate.WiFi => "📶",
        EntryTemplate.SshFtp => "🖥️",
        EntryTemplate.Vpn => "🔐",
        EntryTemplate.Database => "🗄️",
        EntryTemplate.Computer => "💻",
        EntryTemplate.MobileApp => "📱",
        EntryTemplate.IdDocument => "🪪",
        EntryTemplate.LicenseKey => "🔑",
        EntryTemplate.SecureNote => "📝",
        _ => "🔐"
    };

    /// <summary>Nom d'affichage du modèle.</summary>
    public static string ToDisplayName(this EntryTemplate t) => t switch
    {
        EntryTemplate.Email => "Email / Messagerie",
        EntryTemplate.SocialMedia => "Réseau social",
        EntryTemplate.Website => "Site Web / E-commerce",
        EntryTemplate.Gaming => "Jeu vidéo",
        EntryTemplate.Streaming => "Streaming",
        EntryTemplate.BankCard => "Carte bancaire",
        EntryTemplate.BankAccount => "Compte bancaire",
        EntryTemplate.Crypto => "Crypto-monnaie",
        EntryTemplate.WiFi => "WiFi / Réseau",
        EntryTemplate.SshFtp => "Serveur SSH / FTP",
        EntryTemplate.Vpn => "VPN",
        EntryTemplate.Database => "Base de données",
        EntryTemplate.Computer => "Ordinateur / OS",
        EntryTemplate.MobileApp => "Application mobile",
        EntryTemplate.IdDocument => "Identité / Document",
        EntryTemplate.LicenseKey => "Clé de licence",
        EntryTemplate.SecureNote => "Note sécurisée",
        _ => "Entrée standard"
    };

    /// <summary>Catégorie d'appartenance du modèle.</summary>
    public static string ToCategory(this EntryTemplate t) => t switch
    {
        EntryTemplate.Email or EntryTemplate.SocialMedia or
        EntryTemplate.Website or EntryTemplate.Gaming or
        EntryTemplate.Streaming => "Compte",
        EntryTemplate.BankCard or EntryTemplate.BankAccount or
        EntryTemplate.Crypto => "Finance",
        EntryTemplate.WiFi or EntryTemplate.SshFtp or
        EntryTemplate.Vpn or EntryTemplate.Database => "Réseau",
        EntryTemplate.Computer or EntryTemplate.MobileApp => "Appareil",
        EntryTemplate.IdDocument => "Personnel",
        EntryTemplate.LicenseKey => "Logiciel",
        EntryTemplate.SecureNote => "Note",
        _ => "Divers"
    };

    /// <summary>Le modèle a-t-il une date d'expiration pertinente ?</summary>
    public static bool HasExpiry(this EntryTemplate t) => t switch
    {
        EntryTemplate.BankCard or EntryTemplate.Streaming or
        EntryTemplate.LicenseKey or EntryTemplate.IdDocument => true,
        _ => false
    };

    /// <summary>Le modèle a-t-il une URL pertinente ?</summary>
    public static bool HasUrl(this EntryTemplate t) => t switch
    {
        EntryTemplate.BankCard or EntryTemplate.BankAccount or
        EntryTemplate.Crypto or EntryTemplate.WiFi or
        EntryTemplate.Computer or EntryTemplate.IdDocument or
        EntryTemplate.SecureNote => false,
        _ => true
    };
}