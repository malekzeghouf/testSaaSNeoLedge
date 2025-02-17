using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace AuthenticationApi.Infrastructure.Services
{
    public class KeyService
    {
        public (string PublicKey, string PrivateKey) GenerateRsaKeys()
        {
            using var rsa = RSA.Create(2048);

            // Export RSA keys as PEM strings
            var publicKey = PemConverter.ToPem(rsa.ExportSubjectPublicKeyInfo(), "PUBLIC KEY");
            var privateKey = PemConverter.ToPem(rsa.ExportPkcs8PrivateKey(), "PRIVATE KEY");

            return (publicKey, privateKey);
        }
    }

    public static class PemConverter
    {
        public static string ToPem(byte[] data, string type)
        {
            var base64 = Convert.ToBase64String(data, Base64FormattingOptions.InsertLineBreaks);
            return $"-----BEGIN {type}-----\n{base64}\n-----END {type}-----";
        }
    }
}