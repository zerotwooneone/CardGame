namespace CardGame.Domain.Turn;

public record RoundCard(PlayableCard PlayableCard)
{
    private PlayableCard Playable=> PlayableCard;
    public CardId Id => Playable.CardId;
    public Types.CardType Type=> Playable.Type;
    public bool KickOutOfRoundOnDiscard => Playable.KickOutOfRoundOnDiscard;
    
    public override string ToString() => $"{Id}:{Type}";

    public PlayableCard ToPlayableCard()
    {
        return Playable;
    }
}