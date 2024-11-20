namespace CardGame.Domain.Turn;

public readonly struct PlayerId(ulong value) : IEquatable<PlayerId>
{
    public const ulong DefaultValue = ulong.MaxValue;
    
    public ulong Value { get; } = value;

    public PlayerId() : this(DefaultValue) { }

    public static explicit operator PlayerId(ulong v)
    {
        return new PlayerId(v);
    }

    public static implicit operator ulong(PlayerId value)
    {
        return value.Value;
    }

    public override bool Equals(object? obj)
    {
        return Value.Equals(obj);
    }

    public bool Equals(PlayerId other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(PlayerId left, PlayerId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PlayerId left, PlayerId right)
    {
        return !left.Equals(right);
    }
    public override string ToString()
    {
        return $"P{Value.ToString()}";
    }
}