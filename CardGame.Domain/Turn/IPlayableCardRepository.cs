namespace CardGame.Domain.Turn;

public interface IPlayableCardRepository
{
    Task<PlayableCard?> Get(GameId gameId, CardId cardId);
}