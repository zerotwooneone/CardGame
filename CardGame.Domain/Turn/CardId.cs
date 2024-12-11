namespace CardGame.Domain.Turn;

public record struct CardId(uint Value) : IEquatable<CardId>
{
    public uint Value { get; } = Value;

    public static explicit operator CardId(uint v)
    {
        return new CardId(v);
    }

    public override string ToString()
    {
        return $"Cid{Value.ToString()}";
    }
}