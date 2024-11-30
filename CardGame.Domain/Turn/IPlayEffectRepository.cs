namespace CardGame.Domain.Turn;

public interface IPlayEffectRepository
{
    Task<PlayableCard?> Get(GameId gameId, CardId cardId, PlayParams playParams);
}