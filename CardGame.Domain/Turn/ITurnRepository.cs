namespace CardGame.Domain.Turn;

public interface ITurnRepository
{
    Task<Turn?> GetCurrentTurn(GameId gameId);
    Task Save(Turn turn);
}