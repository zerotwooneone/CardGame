using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Domain.Abstractions.Game;
using CardGame.Domain.Card;
using CardGame.Domain.Game;
using CardGame.Domain.Player;
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
                var convertPlayers = ConvertPlayers(gameDao).ToArray();
                _game = Game.Factory(Guid.Parse(converted.Id),
                    convertPlayers,
                    Domain.Round.Round.Factory(int.Parse(converted.Round.Id),
                        Domain.Round.Turn.Factory(int.Parse(converted.Round.Turn.Id),
                            PlayerId.Factory(Guid.Parse(converted.Round.Turn.CurrentPlayer)).Value).Value,
                        Deck.Factory(GetTestDeck()).Value,
                        convertPlayers.Select(p=>p.Id).Except(converted.Round.EliminatedPlayers.Select(p2 => PlayerId.Factory(Guid.Parse(p2)).Value)),
                        Enumerable.Empty<CardId>()
                    ).Value).Value;
            }

            return _game;
        }

        private IEnumerable<CardId> GetTestDeck()
        {
            return new[]
            {
                CardId.Factory(CardStrength.Baron).Value,
                CardId.Factory(CardStrength.Countess).Value,
                CardId.Factory(CardStrength.Guard).Value,
            };
        }

        private IEnumerable<Player> ConvertPlayers(GameDao gameDao)
        {
            var p = new[]
            {
                new {PlayerId = gameDao.Player1, Score = (int?) gameDao.Player1Score, Hand = gameDao.Player1Hand, Protected = false},
                new {PlayerId = gameDao.Player2, Score = (int?) gameDao.Player2Score, Hand = gameDao.Player2Hand, Protected = false},
                new {PlayerId = gameDao.Player3, Score = gameDao.Player3Score, Hand = gameDao.Player3Hand, Protected = false},
                new {PlayerId = gameDao.Player4, Score = gameDao.Player4Score, Hand = gameDao.Player4Hand, Protected = false}
            };
            return p
                .Select(po =>
                {
                    if (string.IsNullOrWhiteSpace(po.PlayerId)) return null;
                    //todo: maybe handle converting hands
                    var player = Player.Factory(Guid.Parse(po.PlayerId), Hand.Factory(new []{CardId.Factory(CardStrength.Guard).Value, CardId.Factory(CardStrength.Handmaid).Value}).Value,
                        Score.Factory(po.Score ?? 0).Value, po.Protected ? ProtectState.Protected : ProtectState.UnProtected).Value;
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