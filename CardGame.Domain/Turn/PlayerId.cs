namespace CardGame.Domain.Turn;

public record struct PlayerId(ulong value) : IEquatable<PlayerId>
{
    public ulong Value { get; } = value;

    public static explicit operator PlayerId(ulong v)
    {
        return new PlayerId(v);
    }
    public override string ToString()
    {
        return $"Pl{Value.ToString()}";
    }
}