namespace CardGame.Domain.Turn;

public class RemainingPlayer(
    PlayerId id, 
    Card hand, 
    bool isProtected=false,
    IEnumerable<Card>? discardPile = null) : ITargetablePlayer
{
    public PlayerId Id { get; } = id;
    public bool IsProtected { get; private set; } = isProtected;
    public Card Hand { get; private set; } = hand;
    public IReadOnlyCollection<Card> DiscardPile => _discardPile;
    private readonly List<Card> _discardPile = discardPile?.ToList() ?? [];
    public Card Trade(Card otherHand)
    {
        var result = Hand;
        Hand = otherHand;
        return result;
    }
    public void RemoveFromRound(Card discarded)
    {   
        _discardPile.Add(Hand);
        _discardPile.Add(discarded);
    }
    public Card DiscardAndDraw(Card card)
    {
        var discarded = Hand;
        _discardPile.Add(discarded);
        Hand = card;
        return discarded;
    }

    public bool Equals(RemainingPlayer? other)
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
        return Equals((RemainingPlayer)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    
    public override string ToString()
    {
        return Id.ToString();
    }

    public void Discard(Card discarded, Card remaining)
    {
        _discardPile.Add(discarded);
        Hand = remaining;
    }

    public void Protect()
    {
        IsProtected = true;
    }

    public void StartTurn()
    {
        IsProtected = false;
    }
}

internal static class RemainingPlayerExtensions
{
    public static RoundPlayer ToEliminated(this RemainingPlayer player)
    {
        return new RoundPlayer(player.Id, player.DiscardPile);
    }
    
    public static CurrentPlayer ToCurrentPlayer(this RemainingPlayer firstPlayer, Card drawnCard)
    {
        return new CurrentPlayer(firstPlayer.Id,firstPlayer.Hand,drawnCard, firstPlayer.IsProtected);
    }
}