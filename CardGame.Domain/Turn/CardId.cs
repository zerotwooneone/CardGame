namespace CardGame.Domain.Turn;

public readonly struct CardId(uint value) : IEquatable<CardId>
{
    public const uint DefaultValue = UInt32.MaxValue;
    
    public uint Value { get; } = value;

    public CardId() : this(DefaultValue) { }

    public static explicit operator CardId(uint v)
    {
        return new CardId(v);
    }

    public static implicit operator uint(CardId value)
    {
        return value.Value;
    }

    public override string ToString()
    {
        return $"C{Value.ToString()}";
    }

    public override bool Equals(object? obj)
    {
        return Value.Equals(obj);
    }

    public bool Equals(CardId other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(CardId left, CardId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CardId left, CardId right)
    {
        return !left.Equals(right);
    }
}