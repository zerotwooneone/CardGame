namespace CardGame.Domain.Turn;

public class CurrentPlayer: IEquatable<CurrentPlayer>, ITargetablePlayer
{
    public CurrentPlayer(
        PlayerId id,
        Card? card1 = null,
        Card? card2 =null, 
        bool isProtected=false)
    {
        if(card1==null && card2==null)
        {
            throw new Exception("player must have at least one card");
        }
        Id = id;
        Card1 = card1;
        Card2 = card2;
    }

    public PlayerId Id { get; }
    public Card? Card1 { get; private set; }
    public Card? Card2 { get; private set; }

    public IReadOnlyCollection<Card> GetHand()
    {
        if (Card1 == null && Card2 != null)
        {
            return new[] {Card2};
        }

        if (Card1 != null && Card2 == null)
        {
            return new[] {Card1};
        }
        
        if(Card1 == null || Card2 == null)
        {
            throw new Exception("cannot get hand when hand is empty");
        }

        return new[] {Card1, Card2};
    }
    public bool IsProtected { get; private set; }

    public void Play(PlayableCard effect, Card card)
    {
        if (!Has(card))
        {
            throw new Exception($"player does not have card {card}");
        }

        var otherCard = GetHand().Single(c => c.Id != card.Id);
        if (effect.PlayProhibitedByCardInHand.Any(p=>p == otherCard.Value))
        {
            throw new Exception($"Playing {card} is prohibited by {otherCard} in hand.");
        }
        if (Card1 == card)
        {
            Card1 = null;
            return;
        }

        if (Card2 == card)
        {
            Card2 = null;
        }
    }

    private bool Has(Card card)
    {
        return Card1 == card || Card2 == card;
    }

    public Card Trade(Card otherHand)
    {
        if (Card1 == null && Card2 != null)
        {
            var card2 = Card2;
            Card2 = otherHand;
            return card2;
        }

        if (Card1 == null || Card2 != null)
        {
            throw new Exception("cannot trade when hand is full or empty");
        }

        var result = Card1;
        Card1 = otherHand;
        return result;
    }

    public Card DiscardAndDraw(Card card)
    {
        var discarded = GetHand().Single();
        Card1 = card;
        return discarded;
    }

    public bool Equals(CurrentPlayer? other)
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
        return Equals((CurrentPlayer)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    
    public override string ToString()
    {
        return Id.ToString();
    }

    public void Protect()
    {
        IsProtected = true;
    }

    public void StartTurn()
    {
        IsProtected = false;
    }

    public void Draw(Card card)
    {
        if (Card1 != null && Card2 != null)
        {
            throw new Exception("cannot draw when hand is full");
        }
        if (Card1 == null)
        {
            Card1 = card;
            return;
        }
        
        Card2 = card;
    }
}