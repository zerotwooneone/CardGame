namespace CardGame.Domain.Turn;

public readonly struct CardValue : IEquatable<CardValue>
{
    public const int MinValue = 1;
    public const int MaxValue = 8;
    public const int DefaultValue = MinValue;
    
    public int Value { get; }

    public CardValue()
    {
        Value = DefaultValue;
    }
    public CardValue(int value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException($"value {value} is not valid");
        }

        Value = value;
    }

    private static bool IsValid(int value)
    {
        return value is >= MinValue and <= MaxValue;
    }

    public static explicit operator CardValue(int v)
    {
        return new CardValue(v);
    }

    public static implicit operator int(CardValue value)
    {
        return value.Value;
    }

    public override string ToString()
    {
        return $"CV{Value}";
    }

    public override bool Equals(object? obj)
    {
        return Value.Equals(obj);
    }

    public bool Equals(CardValue other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public static bool operator ==(CardValue left, CardValue right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CardValue left, CardValue right)
    {
        return !left.Equals(right);
    }
}