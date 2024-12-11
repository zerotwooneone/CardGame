namespace CardGame.Domain.Turn;

public class RemainingPlayer(
    PlayerId id, 
    RoundCard hand, 
    bool isProtected=false,
    IEnumerable<Card>? discardPile = null): IEquatable<RemainingPlayer>
{
    public PlayerId Id { get; } = id;
    public bool IsProtected { get; private set; } = isProtected;
    public RoundCard Hand { get; private set; } = hand;
    public IReadOnlyCollection<Card> DiscardPile => _discardPile;
    private readonly List<Card> _discardPile = discardPile?.ToList() ?? [];
    public RoundCard Trade(RoundCard otherHand)
    {
        var result = Hand;
        Hand = otherHand;
        return result;
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

    public void ReplaceHand(RoundCard discarded, RoundCard hand)
    {
        if(Hand.Id.Equals(hand.Id) && Hand.Id.Equals(discarded.Id))
        {
            throw new Exception("hand does not match");
        }
        if (_discardPile.Select(c => c.Id).Contains(discarded.Id))
        {
            throw new Exception("hand already discarded");
        }

        _discardPile.Add(new Card{Id = discarded.Id, Value = discarded.Value});
        Hand = hand;
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
        return new RoundPlayer(player.Id, player.DiscardPile.Append(new Card{Id = player.Hand.Id, Value = player.Hand.Value}).ToArray());
    }
    
    public static CurrentPlayer ToCurrentPlayer(this RemainingPlayer player, RoundCard drawnCard)
    {
        return new CurrentPlayer(player.Id,player.Hand.ToPlayableCard(),drawnCard.ToPlayableCard());
    }
}