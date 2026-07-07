using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.Identity;

/// <summary>AES-256-GCM with a key derived from config (Encryption:Key, base64 - generate with
/// e.g. `openssl rand -base64 32`). Used only for values that must be decrypted later (the TOTP
/// secret) - passwords use one-way hashing (PasswordHasher) instead.</summary>
public class EncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var configuredKey = configuration["Encryption:Key"];
        _key = string.IsNullOrWhiteSpace(configuredKey)
            ? DevOnlyFallbackKey()
            : Convert.FromBase64String(configuredKey);

        if (_key.Length != 32) throw new InvalidOperationException("Encryption:Key must decode to exactly 32 bytes (AES-256).");
    }

    public string Encrypt(string plaintext)
    {
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var combined = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, 0, combined, NonceSize, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, combined, NonceSize + ciphertext.Length, TagSize);
        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string ciphertextB64)
    {
        var combined = Convert.FromBase64String(ciphertextB64);
        var nonce = combined[..NonceSize];
        var tag = combined[^TagSize..];
        var ciphertext = combined[NonceSize..^TagSize];
        var plaintextBytes = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);
        return System.Text.Encoding.UTF8.GetString(plaintextBytes);
    }

    /// <summary>Stable (not random) so restarts in local dev don't invalidate already-encrypted
    /// TOTP secrets - clearly marked as dev-only; production must set Encryption:Key.</summary>
    private static byte[] DevOnlyFallbackKey() =>
        SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("stlms-dev-only-encryption-key-do-not-use-in-prod"));
}
