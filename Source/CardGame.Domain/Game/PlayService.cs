using System;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Game;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Domain.Game
{
    public class PlayService : IPlayService
    {
        private readonly IBus _bus;

        public PlayService(IBus bus)
        {
            _bus = bus;
        }
        public async Task Play(PlayRequest request)
        {
            var correlationId = request.CorrelationId ?? request.EventId;
            //todo fill this out
            var response = await _bus.Request<NextRoundRequest, RoundStarted>("CardGame.Domain.Abstractions.Game.IGameService", "NextRound", "RoundStarted", request.EventId, new NextRoundRequest
            {
                GameId = request.GameId,
                CorrelationId = correlationId,
                EventId = Guid.NewGuid(),
                WinningPlayer = request.PlayerId
            });
            _bus.Publish("CardPlayed", new PlayResponse
            {
                CorrelationId = correlationId,
                EventId = Guid.NewGuid(),
            },
                correlationId);
        }
    }
}