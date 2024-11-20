namespace CardGame.Domain.Turn;

public readonly struct GameId(ulong value) : IEquatable<GameId>
{
    public const ulong DefaultValue = ulong.MaxValue;
    
    public ulong Value { get; } = value;

    public GameId() : this(DefaultValue) { }

    public static explicit operator GameId(ulong v)
    {
        return new GameId(v);
    }

    public static implicit operator ulong(GameId value)
    {
        return value.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override bool Equals(object? obj)
    {
        return Value.Equals(obj);
    }

    public bool Equals(GameId other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(GameId left, GameId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GameId left, GameId right)
    {
        return !left.Equals(right);
    }
}