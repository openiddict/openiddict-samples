using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OpenIddict.DeviceCodeFlow
{
    public class CodeGenerator : ICodeGenerator
    {
        /// <summary>
        /// Generates a secure random code.
        /// </summary>
        /// <param name="length">Length in characters of the code to generate</param>
        /// <returns></returns>
        public string NewCode(int length)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenData = new byte[(length * 5 + 7) / 8];
                rng.GetBytes(tokenData);

                return ZBase32.Encode(tokenData).Substring(0, length);
            }
        }

        public string NormalizeCode(string code)
        {
            return new string(code.Where(ZBase32.IsValid).ToArray());
        }
    }
}