using System;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpenIddict.DeviceCodeFlow
{

    public static class ZBase32
    {
        public static char[] Alphabet = "ybndrfg8ejkmcpqxot1uwisza345h769".ToCharArray();

        public static bool IsValid(char c)
        {
            return (c >= 'a' && c <= 'z' && c != 'l' && c != 'v') 
                || (c >= '0' && c <= '9' && c != '0' && c != '2');
        }

        public static string Encode(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var result = new StringBuilder((bytes.Length * 8 + 4) / 5);

            for (var i = 0; i < bytes.Length; i += 5)
            {
                var chunkBytes = Math.Min(5, bytes.Length - i);
                var chunkBits = chunkBytes * 8;

                ulong chunk = 0;

                for (var j = 0; j < chunkBytes; ++j)
                {
                    chunk |= (ulong)bytes[i + j] << (8 * (chunkBytes - j - 1));
                }

                for (var k = 0; k < chunkBits; k += 5)
                {
                    var remainingBits = chunkBits - k;
                    var bits = Math.Min(5, remainingBits);

                    var alpha = (int)((chunk >> Math.Max(0, remainingBits - 5))
                        & (ulong)(0x1f >> (5 - bits)))
                        << (5 - bits);

                    result.Append(Alphabet[alpha]);
                }
            }

            return result.ToString();
        }
    }
}