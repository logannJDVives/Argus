using System;
using System.Security.Cryptography;
using System.Text;

namespace Argus.Utilities
{
    public static class SecretMaskingUtility
    {
        public static string MaskSecret(string secret)
        {
            if (string.IsNullOrEmpty(secret))
                return string.Empty;

            if (secret.Length <= 8)
                return new string('*', secret.Length);

            int visibleChars = Math.Min(4, secret.Length / 4);
            int maskLength = secret.Length - visibleChars;
            string visible = secret.Substring(secret.Length - visibleChars);

            return $"{new string('*', maskLength)}{visible}";
        }

        public static string GenerateSHA256Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
