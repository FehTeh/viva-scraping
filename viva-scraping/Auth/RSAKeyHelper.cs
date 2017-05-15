using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace viva.scraping.Auth
{
    public class RSAKeyHelper
    {
        public static RSAParameters GenerateKey()
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = 2048;

                return rsa.ExportParameters(true);
            }
        }
    }
}
