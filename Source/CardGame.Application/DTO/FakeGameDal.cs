using System.Threading.Tasks;
using CardGame.Domain.Abstractions.Game;

namespace CardGame.Application.DTO
{
    public class FakeGameDal : IGameDal
    {
        private GameDao _state = new GameDao
        {
            Id = "96a8f4b0-7800-4c26-80b6-fd66f286140f",
            CurrentPlayer = "9b644228-6c7e-4caa-becf-89e093ee299f",
            TurnId = "2",
            RoundId = "3",
            DeckCount = 9,
            Discard = new[] {"30", "40"},
            EliminatedPlayer1 = null,
            EliminatedPlayer2 = null,
            EliminatedPlayer3 = null,
            Player1 = "9b644228-6c7e-4caa-becf-89e093ee299f",
            Player2 = "5e96fafb-83b2-4e72-8afa-0e6a8f12345f",
            Player3 = null,
            Player4 = null,
            Player1Score = 1,
            Player2Score = 3,
            Player3Score = null,
            Player4Score = null,
            Player1Hand = "10",
            Player2Hand = "20",
            Player3Hand = null,
            Player4Hand = null,
            Player1Protected = false,
            Player2Protected = false,
            Player3Protected = false,
            Player4Protected = false,
        };
        public Task<GameDao> GetById(string id)
        {
            return Task.FromResult(_state);
        }

        public async Task SetById(GameDao gameDao)
        {
            _state = gameDao;
        }
    }
}