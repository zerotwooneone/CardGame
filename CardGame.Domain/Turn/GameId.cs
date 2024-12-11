namespace CardGame.Domain.Turn;

public record struct GameId(ulong Value) : IEquatable<GameId>
{
    public ulong Value { get; } = Value;

    public static explicit operator GameId(ulong v)
    {
        return new GameId(v);
    }

    public override string ToString()
    {
        return $"G{Value}";
    }
}