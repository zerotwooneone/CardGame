using System;
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
                    DeckCount = gameDao.Deck.Split(";").Count(s => !string.IsNullOrWhiteSpace(s)),
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

        public PlayerDto ConvertToPlayer(GameDao gameDao, Guid playerId)
        {
            var converted = ConvertToCommonKnowledgeGame(gameDao);
            var playerIndex = converted.Players.ToList().IndexOf(playerId.ToString());
            
            switch (playerIndex)
            {
                case 0:
                    return new PlayerDto
                    {
                        Hand = gameDao.Player1Hand,
                        Score = gameDao.Player1Score,
                        Protected = gameDao.Player1Protected
                    };
                case 1:
                    return new PlayerDto
                    {
                        Hand = gameDao.Player2Hand,
                        Score = gameDao.Player2Score,
                        Protected = gameDao.Player2Protected
                    };
                case 2:
                    return new PlayerDto
                    {
                        Hand = gameDao.Player3Hand,
                        Score = gameDao.Player3Score,
                        Protected = gameDao.Player3Protected
                    };
                case 3:
                    return new PlayerDto
                    {
                        Hand = gameDao.Player4Hand,
                        Score = gameDao.Player4Score,
                        Protected = gameDao.Player4Protected
                    };
                default:
                    throw new ArgumentException($"player not found {playerId}", nameof(playerId));
            }
        }
    }
}