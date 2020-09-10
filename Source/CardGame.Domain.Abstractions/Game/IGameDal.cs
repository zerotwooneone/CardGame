using System.Threading.Tasks;

namespace CardGame.Domain.Abstractions.Game
{
    public interface IGameDal
    {
        Task<GameDao> GetById(string id);
    }
}
