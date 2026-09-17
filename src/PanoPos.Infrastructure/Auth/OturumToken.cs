using System.Security.Cryptography;
using System.Text;

namespace PanoPos.Infrastructure.Auth;

internal static class OturumToken
{
    public static string Uret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
