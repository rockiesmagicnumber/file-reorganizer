namespace PhotoLibraryCleaner.Lib;

/// <summary>
/// This struct is needed because we are taking checksums and using them as keys in an IDictionary
/// Taken straight from https://stackoverflow.com/a/26554196
/// </summary>
public struct NString
{
    public NString(string value)
        : this()
    {
        Value = value ?? string.Empty;
    }

    public string Value
    {
        get;
        private set;
    }

    public static implicit operator NString(string value)
    {
        return new NString(value);
    }

    public static implicit operator string(NString value)
    {
        return value.Value;
    }
}