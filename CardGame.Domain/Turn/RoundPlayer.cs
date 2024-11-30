namespace CardGame.Domain.Turn;

public class RoundPlayer(PlayerId id, IEnumerable<Card>? discardPile=null)
{
    public PlayerId Id { get; } = id;
    public IReadOnlyCollection<Card> DiscardPile { get; } = discardPile?.ToArray() ?? [];
    public bool Equals(RoundPlayer? other)
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
        return Equals((RoundPlayer)obj);
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