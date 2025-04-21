namespace CardGame.Domain.Turn;

public record PlayableCard
{
    public required CardId CardId { get;  init;}
    public required Types.CardType Type { get; init; }
    public bool CanTargetSelf { get; init; }
    public IReadOnlyCollection<Types.CardType> PlayProhibitedByCardInHand { get; init;} = Array.Empty<Types.CardType>();
    public bool TradeHands { get; init;}
    public bool RequiresTargetPlayer { get; init;}
    public bool KickOutOfRoundOnDiscard { get; init;}
    public bool DiscardAndDraw { get; init;}
    public bool Protect { get; init;}
    public bool Compare { get; init; }
    public bool Inspect { get; init; }
    public bool Guess { get; init; }
}