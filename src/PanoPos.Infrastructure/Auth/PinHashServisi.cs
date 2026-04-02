using System.Security.Cryptography;
using PanoPos.Application.Auth;
using PanoPos.Application.Common;

namespace PanoPos.Infrastructure.Auth;

public sealed class PinHashServisi : IPinHashServisi
{
    private const int SaltBoyutu = 16;
    private const int AnahtarBoyutu = 32;
    private const int Iterasyon = 100_000;

    public string Hashle(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            throw new UygulamaHatasi(400, "Gecersiz PIN", "PIN bos olamaz.", "pin_required");
        }

        Span<byte> salt = stackalloc byte[SaltBoyutu];
        RandomNumberGenerator.Fill(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt.ToArray(), Iterasyon, HashAlgorithmName.SHA256, AnahtarBoyutu);

        return string.Join('.', Iterasyon, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Dogrula(string pin, string hash)
    {
        if (string.IsNullOrWhiteSpace(pin) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            var parts = hash.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterasyon))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[1]);
            var beklenenHash = Convert.FromBase64String(parts[2]);
            var mevcutHash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iterasyon, HashAlgorithmName.SHA256, beklenenHash.Length);

            return CryptographicOperations.FixedTimeEquals(mevcutHash, beklenenHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
