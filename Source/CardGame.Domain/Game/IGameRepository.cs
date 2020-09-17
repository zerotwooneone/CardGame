using System.Threading.Tasks;
using CardGame.Domain.Abstractions.Game;

namespace CardGame.Domain.Game
{
    public interface IGameRepository
    {
        Task<Game> GetById(IGameId id);
        Task SetById(Game game);
    }
}