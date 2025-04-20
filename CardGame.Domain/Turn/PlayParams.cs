namespace CardGame.Domain.Turn;

public record PlayParams
{
    public static PlayParams Default => new() { TargetPlayer = null, Guess = null };
    public PlayerId? TargetPlayer { get; init; }
    public CardType? Guess { get; init; }
}