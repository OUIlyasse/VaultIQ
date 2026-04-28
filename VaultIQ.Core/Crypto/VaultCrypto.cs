using System.Security.Cryptography;
using System.Text;

namespace VaultIQ.Core.Crypto;

/// <summary>
/// Chiffrement AES-256-CBC + PBKDF2-SHA256 + HMAC-SHA256.
/// Aucune dépendance externe — BCL .NET 8 uniquement.
/// </summary>
public static class VaultCrypto
{
    private const int SaltBytes = 32;
    private const int IvBytes = 16;
    private const int HmacBytes = 32;
    private const int KdfIterations = 150_000;
    private const int KeyBytes = 32; // AES-256

    // ── Chiffrement ───────────────────────────────────────────────
    /// <summary>Chiffre des données brutes. Retourne : [salt][iv][hmac][cipher]</summary>
    public static byte[] Encrypt(byte[] data, string password)
    {
        byte[] salt = RandomBytes(SaltBytes);
        byte[] iv = RandomBytes(IvBytes);
        byte[] aesKey = DeriveKey(password, salt, KdfIterations);
        byte[] hmacKey = DeriveKey(password + "_HMAC", salt, KdfIterations / 2);

        byte[] cipher = AesEncrypt(data, aesKey, iv);
        byte[] hmac = ComputeHmac(cipher, hmacKey);

        // Layout : salt(32) + iv(16) + hmac(32) + cipher(N)
        var result = new byte[SaltBytes + IvBytes + HmacBytes + cipher.Length];
        Buffer.BlockCopy(salt, 0, result, 0, SaltBytes);
        Buffer.BlockCopy(iv, 0, result, SaltBytes, IvBytes);
        Buffer.BlockCopy(hmac, 0, result, SaltBytes + IvBytes, HmacBytes);
        Buffer.BlockCopy(cipher, 0, result, SaltBytes + IvBytes + HmacBytes, cipher.Length);
        return result;
    }

    // ── Déchiffrement ──────────────────────────────────────────────
    /// <summary>Déchiffre et vérifie le HMAC. Lève <see cref="CryptographicException"/> si invalide.</summary>
    public static byte[] Decrypt(byte[] data, string password)
    {
        if (data.Length < SaltBytes + IvBytes + HmacBytes)
            throw new CryptographicException("Données trop courtes.");

        byte[] salt = data[..SaltBytes];
        byte[] iv = data[SaltBytes..(SaltBytes + IvBytes)];
        byte[] storedHmac = data[(SaltBytes + IvBytes)..(SaltBytes + IvBytes + HmacBytes)];
        byte[] cipher = data[(SaltBytes + IvBytes + HmacBytes)..];

        byte[] hmacKey = DeriveKey(password + "_HMAC", salt, KdfIterations / 2);
        byte[] actualHmac = ComputeHmac(cipher, hmacKey);

        if (!CryptographicOperations.FixedTimeEquals(storedHmac, actualHmac))
            throw new CryptographicException("Vérification HMAC échouée — mot de passe incorrect ou fichier corrompu.");

        byte[] aesKey = DeriveKey(password, salt, KdfIterations);
        return AesDecrypt(cipher, aesKey, iv);
    }

    // ── Générateur de mots de passe ────────────────────────────────
    /// <summary>Génère un mot de passe aléatoire via CSPRNG.</summary>
    public static string GeneratePassword(
        int length = 20,
        bool uppercase = true,
        bool lowercase = true,
        bool digits = true,
        bool symbols = false,
        bool noAmbiguous = true)
    {
        var pool = new StringBuilder();
        if (uppercase) pool.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        if (lowercase) pool.Append("abcdefghijklmnopqrstuvwxyz");
        if (digits) pool.Append("0123456789");
        if (symbols) pool.Append("!@#$%^&*()-_=+[]{}|;:,.<>?");

        string chars = noAmbiguous
            ? new string(pool.ToString().Where(c => !"0O1lI".Contains(c)).ToArray())
            : pool.ToString();

        if (chars.Length == 0) chars = "abcdefghijklmnopqrstuvwxyz";

        var buf = new byte[4];
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            RandomNumberGenerator.Fill(buf);
            result[i] = chars[(int)(BitConverter.ToUInt32(buf) % (uint)chars.Length)];
        }
        return new string(result);
    }

    // ── Helpers privés ─────────────────────────────────────────────
    private static byte[] DeriveKey(string password, byte[] salt, int iterations)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeyBytes);
    }

    private static byte[] ComputeHmac(byte[] data, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }

    private static byte[] AesEncrypt(byte[] data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key; aes.IV = iv; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            cs.Write(data);
        return ms.ToArray();
    }

    private static byte[] AesDecrypt(byte[] data, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key; aes.IV = iv; aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var out_ms = new MemoryStream();
        cs.CopyTo(out_ms);
        return out_ms.ToArray();
    }

    private static byte[] RandomBytes(int count)
    {
        var buf = new byte[count];
        RandomNumberGenerator.Fill(buf);
        return buf;
    }
}