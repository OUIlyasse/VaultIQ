using System.IO.Compression;
using System.Text;
using System.Text.Json;
using VaultIQ.Core.Crypto;
using VaultIQ.Core.Entities;

namespace VaultIQ.Core.Storage;

/// <summary>
/// Format de fichier .viq v1 — lecture et écriture.
///
/// Structure binaire :
///   [Magic 8o "VAULTIQ\x01"][Version uint16 0x0100]
///   [NameLen uint16][Name UTF-8 N]
///   [PayloadLen uint32][AES-256(GZip(JSON)) P]
/// </summary>
public static class VaultFile
{
    private static readonly byte[] MagicBytes = "VAULTIQ\x01"u8.ToArray();
    private const ushort FileVersion = 0x0100; // v1.0
    public const string Extension = ".viq";

    // ── Sauvegarde ────────────────────────────────────────────────
    public static void Save(VaultDatabase db, string filePath, string masterPassword)
    {
        string json = JsonSerializer.Serialize(db, _opts);
        byte[] raw = Encoding.UTF8.GetBytes(json);
        byte[] gzipped = GzipCompress(raw);
        byte[] payload = VaultCrypto.Encrypt(gzipped, masterPassword);

        byte[] nameBytes = Encoding.UTF8.GetBytes(db.Name);

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);

        bw.Write(MagicBytes);               // 8 octets
        bw.Write(FileVersion);              // 2 octets
        bw.Write((ushort)nameBytes.Length); // 2 octets
        bw.Write(nameBytes);                // N octets
        bw.Write((uint)payload.Length);     // 4 octets
        bw.Write(payload);                  // P octets
    }

    // ── Chargement ────────────────────────────────────────────────
    public static VaultDatabase Load(string filePath, string masterPassword)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);

        // Vérifier magic bytes
        byte[] magic = br.ReadBytes(8);
        if (!magic.AsSpan().SequenceEqual(MagicBytes.AsSpan()))
            throw new InvalidDataException("Ce fichier n'est pas une base VaultIQ valide.");

        ushort version = br.ReadUInt16();
        if (version > FileVersion)
            throw new InvalidDataException($"Version de fichier non supportée (v{version >> 8}.{version & 0xFF}).");

        ushort nameLen = br.ReadUInt16();
        string _ = Encoding.UTF8.GetString(br.ReadBytes(nameLen)); // Nom public
        uint payLen = br.ReadUInt32();
        byte[] payload = br.ReadBytes((int)payLen);

        // Déchiffrer (lève CryptographicException si mdp incorrect)
        byte[] gzipped = VaultCrypto.Decrypt(payload, masterPassword);
        byte[] raw = GzipDecompress(gzipped);
        string json = Encoding.UTF8.GetString(raw);

        return JsonSerializer.Deserialize<VaultDatabase>(json, _opts)
            ?? throw new InvalidDataException("Impossible de désérialiser la base de données.");
    }

    // ── Lecture du nom public (sans mot de passe) ─────────────────
    /// <summary>
    /// Lit uniquement le nom de la base sans déchiffrer.
    /// Affiché dans la fenêtre de connexion avant saisie du mot de passe.
    /// </summary>
    public static string ReadPublicName(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);
            byte[] magic = br.ReadBytes(8);
            if (!magic.AsSpan().SequenceEqual(MagicBytes.AsSpan())) return "Fichier invalide";
            br.ReadUInt16(); // version
            ushort nameLen = br.ReadUInt16();
            return Encoding.UTF8.GetString(br.ReadBytes(nameLen));
        }
        catch { return Path.GetFileNameWithoutExtension(filePath); }
    }

    public static bool IsValidViqFile(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var buf = new byte[8];
            fs.Read(buf);
            return buf.AsSpan().SequenceEqual(MagicBytes.AsSpan());
        }
        catch { return false; }
    }

    // ── GZip helpers ──────────────────────────────────────────────
    private static byte[] GzipCompress(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionLevel.Optimal)) gz.Write(data);
        return ms.ToArray();
    }

    private static byte[] GzipDecompress(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var gz = new GZipStream(ms, CompressionMode.Decompress);
        using var out_ms = new MemoryStream();
        gz.CopyTo(out_ms);
        return out_ms.ToArray();
    }

    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };
}