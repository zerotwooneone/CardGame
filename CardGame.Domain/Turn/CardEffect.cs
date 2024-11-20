namespace CardGame.Domain.Turn;

public record CardEffect
{
    public CardId Card { get;  init;}
    public bool CanTargetSelf { get; init; }
    public bool CanDiscard { get; init;}
    public bool TradeHands { get; init;}
    public bool RequiresTargetPlayer { get; init;}
    public bool KickOutOfRoundOnDiscard { get; init;}
    public bool DiscardAndDraw { get; init;}
    public bool DiscardAndDrawKickEnabled { get; init;}
    public bool Protect { get; init;}
    public bool Compare { get; init; }
    public bool Inspect { get; init; }
    public bool Guess { get; init; }
}