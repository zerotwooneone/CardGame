using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Application.Client;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Game;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Application.Bus
{
    public class ClientRouter
    {
        private readonly IBus _bus;
        private readonly IGameRepository _gameRepository;
        private readonly ClientHub _clientHub;

        public ClientRouter(IBus bus,
            IGameRepository gameRepository,
            ClientHub clientHub)
        {
            _bus = bus;
            _gameRepository = gameRepository;
            _clientHub = clientHub;
        }

        public void Init()
        {
            var gameStateSubscription = _bus.Subscribe<GameStateChanged>(nameof(GameStateChanged), OnGameStateChanged);
            var commonSubscription = _bus.Subscribe<CommonGameStateChanged>(nameof(CommonGameStateChanged), OnCommonGameStateChanged);
        }

        private async Task OnCommonGameStateChanged(CommonGameStateChanged arg)
        {
            //todo come up with a generic way to publish events to the client
            var clientEvent = new ClientEvent
            {
                EventId = Guid.NewGuid(),
                CorrelationId = arg.CorrelationId,
                GameId = arg.GameId,
                Type = typeof(CommonGameStateChanged).ToString(),
                Topic = nameof(OnCommonGameStateChanged),
                Data = arg
            };
            await _clientHub.SendClientEvent(clientEvent);
        }

        //todo: this probably belongs on the game service
        private async Task OnGameStateChanged(GameStateChanged gameStateChanged)
        {
            //todo what to do if gameid is not good?
            var gameId = GameId.Factory(gameStateChanged.GameId);
            var game = await _gameRepository.GetById(gameId.Value);
            var roundRemainingPlayers = game.Round.RemainingPlayers.ToArray();
            var player1 = game.Players.Skip(0).FirstOrDefault();
            var player2 = game.Players.Skip(1).FirstOrDefault();
            var player3 = game.Players.Skip(2).FirstOrDefault();
            var player4 = game.Players.Skip(3).FirstOrDefault();
            _bus.Publish(nameof(CommonGameStateChanged),new CommonGameStateChanged
            {
                Round = game.Round.Id,
                Discard = game.Round.Discard.Select(cid => $"{cid.CardValue.Value}{cid.Variant}").ToArray(),
                Turn = game.Round.Turn.Id,
                WinningPlayer = game.WinningPlayer?.Value,
                CurrentPlayer = game.Round.Turn.CurrentPlayer.Value,
                Player1Score = player1?.Score?.Value,
                Player2Score = player2?.Score?.Value,
                Player3Score = player3?.Score?.Value,
                Player4Score = player4?.Score?.Value,
                Player1InRound = roundRemainingPlayers.Contains(player1?.Id),
                Player2InRound = roundRemainingPlayers.Contains(player2?.Id),
                Player3InRound = roundRemainingPlayers.Contains(player3?.Id),
                Player4InRound = roundRemainingPlayers.Contains(player4?.Id),
                CorrelationId = gameStateChanged.CorrelationId,
                GameId = game.Id.Value,
                DrawCount = game.Round.Deck.Cards.Count(),
            });
        }
    }
}