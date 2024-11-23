namespace CardGame.Domain.Turn;

public interface IPlayEffectRepository
{
    Task<PlayEffect?> Get(GameId gameId, CardId cardId, PlayParams playParams);
}