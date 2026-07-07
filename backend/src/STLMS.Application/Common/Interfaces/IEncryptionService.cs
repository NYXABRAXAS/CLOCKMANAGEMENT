namespace STLMS.Application.Common.Interfaces;

/// <summary>Symmetric encryption for values that must be decrypted later (e.g. the TOTP secret,
/// which - unlike a password - has to be readable again to verify a 6-digit code).</summary>
public interface IEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
