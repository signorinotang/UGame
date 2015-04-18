using System;

public class IntCachedWrapper
{
    private int cached;

    private int latest;

    public int Cached { get { return cached; } }

    public int Value { get { return latest; } }

    public double Ratio { get { return (double)latest / cached; } }

    public IntCachedWrapper(int value)
    {
        cached = latest = value;
    }

    public IntCachedWrapper(IntCachedWrapper value)
    {
        cached = latest = value.Value;
    }

    public IntCachedWrapper Assign(int value)
    {
        latest = value;

        return this;
    }

    public static implicit operator int(IntCachedWrapper data)
    {
        return data.latest;
    }
}

public class IntCryptoCachedWrapper
{
    private IntCryptoWrapper cached;

    private IntCryptoWrapper latest;

    public int Cached { get { return cached.Value; } }

    public int Value { get { return latest.Value; } }

    public double Ratio { get { return (double)latest.Value / cached.Value; } }

    public IntCryptoCachedWrapper(int value)
    {
        cached = new IntCryptoWrapper(value);
        latest = new IntCryptoWrapper(value);
    }

    public IntCryptoCachedWrapper(IntCryptoCachedWrapper value)
    {
        cached = new IntCryptoWrapper(value.Value);
        latest = new IntCryptoWrapper(value.Value);
    }

    public IntCryptoCachedWrapper Assign(int value)
    {
        latest = new IntCryptoWrapper(value);

        return this;
    }

    public static implicit operator int(IntCryptoCachedWrapper data)
    {
        return data.latest.Value;
    }
}
