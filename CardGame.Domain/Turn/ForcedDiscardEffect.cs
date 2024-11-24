namespace CardGame.Domain.Turn;

public record ForcedDiscardEffect
{
    public bool KickOutOfRoundOnDiscard { get; init;}
    public bool DiscardAndDrawKickEnabled { get; init;}
}