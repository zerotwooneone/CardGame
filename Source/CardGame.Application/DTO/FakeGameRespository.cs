using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Domain.Abstractions.Game;
using CardGame.Domain.Card;
using CardGame.Domain.Game;
using CardGame.Domain.Round;

namespace CardGame.Application.DTO
{
    public class FakeGameRespository : IGameRepository
    {
        private readonly IGameDal _gameDal;
        private readonly IGameConverter _gameConverter;
        
        //todo: replace this hack
        private static Game _game;

        public FakeGameRespository(IGameDal gameDal,
            IGameConverter gameConverter)
        {
            _gameDal = gameDal;
            _gameConverter = gameConverter;
        }
        public async Task<Game> GetById(GameId id)
        {
            var gameDao = await _gameDal.GetById(id.ToString());
            return Convert(gameDao);
        }

        private Game Convert(GameDao gameDao)
        {
            if (_game is null)
            {
                var converted = _gameConverter.ConvertToCommonKnowledgeGame(gameDao);
                _game = Game.Factory(Guid.Parse(converted.Id),
                    ConvertPlayers(gameDao),
                    Domain.Round.Round.Factory(int.Parse(converted.Round.Id),
                        Domain.Round.Turn.Factory(int.Parse(converted.Round.Turn.Id),
                            PlayerId.Factory(Guid.Parse(converted.Round.Turn.CurrentPlayer)).Value).Value,
                        DrawDeckCount.Factory(converted.Round.DeckCount).Value,
                        converted.Round.EliminatedPlayers.Select(p => PlayerId.Factory(Guid.Parse(p)).Value),
                        Enumerable.Empty<CardId>()
                    ).Value).Value;
            }

            return _game;
        }

        private IEnumerable<Player> ConvertPlayers(GameDao gameDao)
        {
            var p = new[]
            {
                new {PlayerId = gameDao.Player1, Score = (int?) gameDao.Player1Score, Hand = gameDao.Player1Hand},
                new {PlayerId = gameDao.Player2, Score = (int?) gameDao.Player2Score, Hand = gameDao.Player2Hand},
                new {PlayerId = gameDao.Player3, Score = gameDao.Player3Score, Hand = gameDao.Player3Hand},
                new {PlayerId = gameDao.Player4, Score = gameDao.Player4Score, Hand = gameDao.Player4Hand}
            };
            return p
                .Select(po =>
                {
                    if (string.IsNullOrWhiteSpace(po.PlayerId)) return null;
                    //todo: maybe handle converting hands
                    var player = Player.Factory(Guid.Parse(po.PlayerId), Hand.Factory().Value,
                        Score.Factory(po.Score ?? 0).Value).Value;
                    return player;
                })
                .Where(p => p != null);
        }

        public async Task SetById(Game game)
        {
            _game = game;
        }
    }

    
}