using CardGame.Domain.Abstractions.Game;

namespace CardGame.Application.DTO
{
    public interface IGameConverter
    {
        Game ConvertGame(GameDao gameDao);
    }
}