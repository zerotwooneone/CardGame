namespace CardGame.Domain.Turn;

public record PlayEffect
{
    public CardId Card { get;  init;}
    public bool CanTargetSelf { get; init; }
    public IReadOnlyCollection<CardValue> PlayProhibitedByCardInHand { get; init;} = Array.Empty<CardValue>();
    public bool TradeHands { get; init;}
    public bool RequiresTargetPlayer { get; init;}
    public bool KickOutOfRoundOnDiscard { get; init;}
    public bool DiscardAndDraw { get; init;}
    public bool Protect { get; init;}
    public bool Compare { get; init; }
    public bool Inspect { get; init; }
    public bool Guess { get; init; }
}