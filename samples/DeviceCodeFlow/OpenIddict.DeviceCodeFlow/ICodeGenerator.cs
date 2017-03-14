using System;

namespace OpenIddict.DeviceCodeFlow
{
    public interface ICodeGenerator
    {
        /// <summary>
        /// Generates a secure random code.
        /// </summary>
        /// <param name="length">Length in characters of the code to generate</param>
        /// <returns></returns>
        string NewCode(int length);
    }
}