using System.Threading.Tasks;

namespace CardGame.Domain.Abstractions.Game
{
    public interface IGameRepository
    {
        Task<GameDao> GetById(string id);
    }
}
