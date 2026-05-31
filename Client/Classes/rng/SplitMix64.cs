
using System;

public sealed class SplitMix64
{
    private ulong state;

    public SplitMix64(ulong seed)
    {
        state = seed;
    }

    public ulong NextUInt64()
    {
        state += 0x9E3779B97F4A7C15UL;

        ulong z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    public int NextInt(int maxExclusive)
    {
        if (maxExclusive <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxExclusive));

        return (int)(NextUInt64() % (ulong)maxExclusive);
    }

    public double NextDouble()
    {
        return (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);
    }
}