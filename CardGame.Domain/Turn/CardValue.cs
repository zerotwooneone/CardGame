namespace CardGame.Domain.Turn;

public record struct CardValue : IEquatable<CardValue>
{
    public const int MinValue = 1;
    public const int MaxValue = 8;
    
    public int Value { get; }
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

    public override string ToString()
    {
        return $"CV{Value}";
    }
}