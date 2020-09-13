using System;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Game;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Domain.Game
{
    public class GameService: IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IBus _bus;

        public GameService(IGameRepository gameRepository,
            IBus bus)
        {
            _gameRepository = gameRepository;
            _bus = bus;
        }

        public async Task NextRound(NextRoundRequest request)
        {
            var gameIdResult = GameId.Factory(request.GameId);
            if (gameIdResult.IsError)
            {
                throw new Exception(gameIdResult.ErrorMessage);
            }

            var playerIdResult = PlayerId.Factory(request.WinningPlayer);
            if (playerIdResult.IsError)
            {
                throw new Exception(playerIdResult.ErrorMessage);
            }

            var gid = gameIdResult.Value;
            var game = await _gameRepository.GetById(gid);
            await game.NextRound(playerIdResult.Value);
            await _gameRepository.SetById(game);

            var roundStarted = new RoundStarted(game.Id.Value, game.Round.Id, request.CorrelationId);
            _bus.PublishEvent("RoundStarted",roundStarted);
        }
    }
}
