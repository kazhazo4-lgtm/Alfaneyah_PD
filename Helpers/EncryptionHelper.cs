using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace ProjectsDashboards.Helpers
{
    public class EncryptionHelper
    {
        private readonly IDataProtector _protector;

        public EncryptionHelper(IDataProtectionProvider provider)
        {
            // Create a protector with a unique purpose string for prices
            _protector = provider.CreateProtector("ProjectsDashboards.PriceEncryption.v1");
        }

        // Encrypt a decimal value for database storage
        public string EncryptPrice(decimal price)
        {
            var plainText = price.ToString("F2"); // Keep 2 decimal places
            var encryptedBytes = _protector.Protect(Encoding.UTF8.GetBytes(plainText));
            return Convert.ToBase64String(encryptedBytes);
        }

        // Encrypt nullable decimal
        public string? EncryptPriceNullable(decimal? price)
        {
            if (!price.HasValue)
                return null;
            return EncryptPrice(price.Value);
        }

        // Decrypt a stored value back to decimal
        public decimal DecryptPrice(string encryptedPrice)
        {
            if (string.IsNullOrEmpty(encryptedPrice))
                return 0;

            var encryptedBytes = Convert.FromBase64String(encryptedPrice);
            var decryptedBytes = _protector.Unprotect(encryptedBytes);
            var plainText = Encoding.UTF8.GetString(decryptedBytes);
            return decimal.Parse(plainText);
        }

        // Decrypt nullable decimal
        public decimal? DecryptPriceNullable(string encryptedPrice)
        {
            if (string.IsNullOrEmpty(encryptedPrice))
                return null;
            return DecryptPrice(encryptedPrice);
        }
    }
}