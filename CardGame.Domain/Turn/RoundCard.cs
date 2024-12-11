namespace CardGame.Domain.Turn;

public record RoundCard(PlayableCard PlayableCard)
{
    private PlayableCard Playable=> PlayableCard;
    public CardId Id => Playable.CardId;
    public CardValue Value=> Playable.Value;
    public bool KickOutOfRoundOnDiscard => Playable.KickOutOfRoundOnDiscard;
    
    public override string ToString() => $"{Id}:{Value}";

    public PlayableCard ToPlayableCard()
    {
        return Playable;
    }
}