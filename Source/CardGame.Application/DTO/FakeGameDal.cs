using System.Threading.Tasks;
using CardGame.Domain.Abstractions.Game;

namespace CardGame.Application.DTO
{
    public class FakeGameDal : IGameDal
    {
        private readonly GameDao _state = new GameDao
        {
            Id = "96a8f4b0-7800-4c26-80b6-fd66f286140f",
            CurrentPlayer = "9b644228-6c7e-4caa-becf-89e093ee299f",
            TurnId = "2",
            RoundId = "3",
            DeckCount = 9,
            Discard = new[] {"discard 1", "discard 2"},
            EliminatedPlayer1 = null,
            EliminatedPlayer2 = null,
            EliminatedPlayer3 = null,
            Player1 = "9b644228-6c7e-4caa-becf-89e093ee299f",
            Player2 = "5e96fafb-83b2-4e72-8afa-0e6a8f12345f",
            Player3 = null,
            Player4 = null
        };
        public Task<GameDao> GetById(string id)
        {
            return Task.FromResult(_state);
        }
    }
}