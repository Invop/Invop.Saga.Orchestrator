using System.Security.Cryptography;
using System.Text;

namespace Invop.Saga.Orchestrator.Core.Extensions;

internal static class StringExtensions
{
    extension(string rawData)
    {
        public string ComputeSha256Hash()
        {
            ArgumentException.ThrowIfNullOrEmpty(rawData);

            ReadOnlySpan<byte> source = Encoding.UTF8.GetBytes(rawData);
            Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes];

            if (!SHA256.TryHashData(source, hashBytes, out _))
            {
                throw new CryptographicException("Failed to compute SHA256 hash.");
            }

            Span<char> hex = stackalloc char[hashBytes.Length * 2];
            for (var i = 0; i < hashBytes.Length; i++)
            {
                var b = hashBytes[i];
                hex[i * 2] = GetHexChar(b >> 4);
                hex[i * 2 + 1] = GetHexChar(b & 0xF);
            }

            return new string(hex);
        }
    }
    private static char GetHexChar(int value) =>
        (char)(value < 10 ? '0' + value : 'a' + (value - 10));
}
