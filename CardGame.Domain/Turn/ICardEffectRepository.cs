namespace CardGame.Domain.Turn;

public interface ICardEffectRepository
{
    Task<CardEffect?> Get(GameId gameId, CardId cardId, PlayParams playParams);
}