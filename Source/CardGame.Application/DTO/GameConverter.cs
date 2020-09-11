using System.Linq;
using CardGame.Domain.Abstractions.Game;

namespace CardGame.Application.DTO
{
    public class GameConverter : IGameConverter
    {
        public CommonKnowledgeGame ConvertToCommonKnowledgeGame(GameDao gameDao)
        {
            return new CommonKnowledgeGame
            {
                Id = gameDao.Id,
                Players = (new[] { gameDao.Player1, gameDao.Player2, gameDao.Player3, gameDao.Player4 }).Where(p => p != null),
                Round = new Round
                {
                    Id = gameDao.RoundId,
                    DeckCount = gameDao.DeckCount,
                    Turn = new Turn
                    {
                        Id = gameDao.TurnId,
                        CurrentPlayer = gameDao.CurrentPlayer
                    },
                    Discard = gameDao.Discard,
                    EliminatedPlayers = (new[] { gameDao.EliminatedPlayer1, gameDao.EliminatedPlayer2, gameDao.EliminatedPlayer3 }).Where(p => p != null)
                }
            };
        }
    }
}