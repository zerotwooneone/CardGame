namespace CardGame.Domain.Turn;

public class CurrentPlayer: IEquatable<CurrentPlayer>
{
    public CurrentPlayer(
        PlayerId id,
        PlayableCard? card1 = null,
        PlayableCard? card2 =null)
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
    public PlayableCard? Card1 { get; private set; }
    public PlayableCard? Card2 { get; private set; }

    public IReadOnlyCollection<PlayableCard> GetHand()
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

    public void Play(PlayableCard played)
    {
        var card = GetHand().Single(c=> c.CardId == played.CardId);

        var otherCard = GetHand().Single(c => c.CardId != card.CardId);
        if (played.PlayProhibitedByCardInHand.Any(p=>p == otherCard.Value))
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

    public PlayableCard Trade(PlayableCard otherHand)
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
    public void Discard(Card card)
    {
        var isCard1 = Card1?.CardId == card.Id && Card1 != null;
        var isCard2 = Card2?.CardId == card.Id && Card2 != null;
        if (!isCard1 && !isCard2)
        {
            throw new Exception($"player does not have card {card}");
        }

        if (isCard1)
        {
            Card1 = null;
            return;
        }

        if (!isCard2)
        {
            throw new Exception($"player does not have card {card}");
        }

        Card2 = null;
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

    public void Draw(PlayableCard card)
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