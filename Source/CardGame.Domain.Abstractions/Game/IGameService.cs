using System.Threading.Tasks;

namespace CardGame.Domain.Abstractions.Game
{
    public interface IGameService
    {
        Task Play(PlayRequest request);
    }
}