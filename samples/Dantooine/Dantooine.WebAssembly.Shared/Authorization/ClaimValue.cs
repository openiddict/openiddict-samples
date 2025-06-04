namespace Dantooine.WebAssembly.Shared.Authorization;

// Original source: https://github.com/berhir/BlazorWebAssemblyCookieAuth.
public class ClaimValue
{
    public ClaimValue(string type, string value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; }

    public string Value { get; }
}
