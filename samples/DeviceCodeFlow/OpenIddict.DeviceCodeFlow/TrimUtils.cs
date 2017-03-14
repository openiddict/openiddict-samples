using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OpenIddict.DeviceCodeFlow
{
    public static class TrimUtils
    {
        public static StringSegment TrimStart(StringSegment segment, char[] separators)
        {
            Debug.Assert(separators?.Length != 0, "The separators collection shouldn't be null or empty.");

            var index = segment.Offset;

            while (index < segment.Offset + segment.Length)
            {
                if (!IsSeparator(segment.Buffer[index], separators))
                {
                    break;
                }

                index++;
            }

            return new StringSegment(segment.Buffer, index, segment.Offset + segment.Length - index);
        }

        private static StringSegment TrimEnd(StringSegment segment, char[] separators)
        {
            Debug.Assert(separators?.Length != 0, "The separators collection shouldn't be null or empty.");

            var index = segment.Offset + segment.Length - 1;

            while (index >= segment.Offset)
            {
                if (!IsSeparator(segment.Buffer[index], separators))
                {
                    break;
                }

                index--;
            }

            return new StringSegment(segment.Buffer, segment.Offset, index - segment.Offset + 1);
        }

        public static StringSegment Trim(StringSegment segment, char[] separators)
        {
            Debug.Assert(separators?.Length != 0, "The separators collection shouldn't be null or empty.");

            return TrimEnd(TrimStart(segment, separators), separators);
        }

        public static bool IsSeparator(char character, char[] separators)
        {
            Debug.Assert(separators?.Length != 0, "The separators collection shouldn't be null or empty.");

            for (var index = 0; index < separators.Length; index++)
            {
                if (character == separators[index])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
