namespace CardGame.Domain.Turn;

public class GamePlayer(PlayerId id)
{
    public PlayerId Id { get; } = id;
    public int Tokens { get; private set; }
    
    public void WinRound()
    {
        Tokens++;
    }
    public bool Equals(GamePlayer? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }
    
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GamePlayer)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    
    public override string ToString()
    {
        return Id.ToString();
    }
}