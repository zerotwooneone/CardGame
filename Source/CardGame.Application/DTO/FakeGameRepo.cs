using System.Threading.Tasks;
using CardGame.Domain.Abstractions.Game;

namespace CardGame.Application.DTO
{
    public class FakeGameRepo : IGameRepository
    {
        private readonly GameDao _state = new GameDao
        {
            Id = "some fake id",
            CurrentPlayer = "some player",
            TurnId = "turn id",
            RoundId = "round id",
            DeckCount = 9,
            EliminatedPlayer1 = "eliminated player 1",
            Player1 = "player 1",
            Discard = new[] {"discard 1", "discard 2"},
            EliminatedPlayer2 = null,
            EliminatedPlayer3 = null,
            Player2 = "player 2",
            Player3 = null,
            Player4 = null
        };
        public Task<GameDao> GetById(string id)
        {
            return Task.FromResult(_state);
        }
    }
}