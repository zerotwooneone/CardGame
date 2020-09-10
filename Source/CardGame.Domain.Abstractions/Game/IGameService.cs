using System.Threading.Tasks;
using CardGame.CommonModel.Bus;

namespace CardGame.Domain.Abstractions.Game
{
    public interface IGameService
    {
        Task NextRound(NextRoundRequest request);
    }
}