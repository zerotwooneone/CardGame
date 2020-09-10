using System;
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
        private Domain.Game.Game _game;

        public FakeGameRespository(IGameDal gameDal,
            IGameConverter gameConverter)
        {
            _gameDal = gameDal;
            _gameConverter = gameConverter;
        }
        public async Task<Domain.Game.Game> GetById(GameId id)
        {
            return Convert(await _gameDal.GetById(id.ToString()));
        }

        private Domain.Game.Game Convert(GameDao gameDao)
        {
            if (_game is null)
            {
                var converted = _gameConverter.ConvertGame(gameDao);
                _game = Domain.Game.Game.Factory(GameId.Factory(Guid.Parse(converted.Id)).Value,
                    converted.Players.Select(p => PlayerId.Factory(Guid.Parse(p)).Value),
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

        public async Task SetById(Domain.Game.Game game)
        {
            _game = game;
        }
    }

    
}