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
                Id = Guid.Parse(gameDao.Id),
                Players = (new[]
                {
                    GetPlayer( id: gameDao.Player1, score: gameDao.Player1Score ),
                    GetPlayer( id: gameDao.Player2, score: gameDao.Player2Score ),
                    GetPlayer( id: gameDao.Player3, score: gameDao.Player3Score ), 
                    GetPlayer( id: gameDao.Player4, score: gameDao.Player4Score ),
                }).Where(p => p != null),
                WinningPlayer = gameDao.WinningPlayer,
                Round = new CommonKnowledgeRound
                {
                    Id = gameDao.RoundId,
                    DeckCount = gameDao.Deck.Split(";").Count(s => !string.IsNullOrWhiteSpace(s)),
                    Turn = new CommonKnowledgeTurn
                    {
                        Id = gameDao.TurnId,
                        CurrentPlayer = gameDao.CurrentPlayer
                    },
                    Discard = gameDao.Discard.Split(";"),
                    PlayerOrder = gameDao.PlayerOrder.Split(";"),
                }
            };
        }

        private CommonKnowledgePlayer GetPlayer(string id, in int? score)
        {
            if (string.IsNullOrWhiteSpace(id) || score == null) return null;
            return new CommonKnowledgePlayer
            {
                Id = Guid.Parse(id),
                Score = score.Value
            };
        }

        public PlayerDto ConvertToPlayer(GameDao gameDao, Guid playerId)
        {
            var converted = ConvertToCommonKnowledgeGame(gameDao);
            var playerIndex = converted.Players.Select(p => p.Id).ToList().IndexOf(playerId);
            
            switch (playerIndex)
            {
                case 0:
                    return new PlayerDto
                    {
                        Hand = CardDto.Create(gameDao.Player1Hand),
                        Score = gameDao.Player1Score,
                        Protected = gameDao.Player1Protected
                    };
                case 1:
                    return new PlayerDto
                    {
                        Hand = CardDto.Create(gameDao.Player2Hand),
                        Score = gameDao.Player2Score,
                        Protected = gameDao.Player2Protected
                    };
                case 2:
                    return new PlayerDto
                    {
                        Hand = CardDto.Create(gameDao.Player3Hand),
                        Score = gameDao.Player3Score,
                        Protected = gameDao.Player3Protected
                    };
                case 3:
                    return new PlayerDto
                    {
                        Hand = CardDto.Create(gameDao.Player4Hand),
                        Score = gameDao.Player4Score,
                        Protected = gameDao.Player4Protected
                    };
                default:
                    throw new ArgumentException($"player not found {playerId}", nameof(playerId));
            }
        }
    }
}