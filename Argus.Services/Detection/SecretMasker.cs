using System;
using System.Security.Cryptography;
using System.Text;

namespace Argus.Services.Detection
{
    public static class SecretMasker
    {
        /// <summary>
        /// Maskeert een secret-waarde: eerste 4 + "****" + laatste 2 karakters.
        /// Korte waarden worden volledig gemaskeerd.
        /// </summary>
        public static string MaskValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "****";

            // Waarden korter dan 8 karakters volledig maskeren
            if (value.Length < 8)
                return "****";

            var prefix = value[..4];
            var suffix = value[^2..];
            return $"{prefix}****{suffix}";
        }

        /// <summary>
        /// Berekent een SHA-256 hash van de waarde als hexadecimale string.
        /// Wordt gebruikt voor unieke identificatie zonder het echte secret op te slaan.
        /// </summary>
        public static string HashValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var bytes  = Encoding.UTF8.GetBytes(value);
            var hash   = SHA256.HashData(bytes);
            return Convert.ToHexStringLower(hash);
        }
    }
}
