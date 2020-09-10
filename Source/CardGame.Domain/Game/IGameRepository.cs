using System.Threading.Tasks;

namespace CardGame.Domain.Game
{
    public interface IGameRepository
    {
        Task<Game> GetById(GameId id);
        Task SetById(Game game);
    }
}