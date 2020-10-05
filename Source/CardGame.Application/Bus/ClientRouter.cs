using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Application.Client;
using CardGame.Application.DTO;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Game;
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
            var playerConnectedSubscription =
                _bus.Subscribe<PlayerConnected>(nameof(PlayerConnected), OnPlayerConnected);
        }

        private async Task OnPlayerConnected(PlayerConnected arg)
        {
            var commonGameStateChanged = await GetCommonGameStateChanged(arg.GameId, arg.CorrelationId);
            var clientEvent = CreateClientEvent(commonGameStateChanged);
            await _clientHub.SendToPlayers(new[] {arg.PlayerId}, clientEvent);
        }

        private async Task OnCommonGameStateChanged(CommonGameStateChanged arg)
        {
            //todo come up with a generic way to publish events to the client
            var clientEvent = CreateClientEvent(arg);
            await _clientHub.SendClientEvent(clientEvent);
        }

        private ClientEvent CreateClientEvent(CommonGameStateChanged arg)
        {
            var clientEvent = new ClientEvent
            {
                EventId = Guid.NewGuid(),
                CorrelationId = arg.CorrelationId,
                GameId = arg.GameId,
                Type = typeof(CommonGameStateChanged).ToString(),
                Topic = nameof(OnCommonGameStateChanged),
                Data = arg
            };
            return clientEvent;
        }

        //todo: this probably belongs on the game service
        private async Task OnGameStateChanged(GameStateChanged gameStateChanged)
        {
            var commonGameStateChanged = await GetCommonGameStateChanged(gameStateChanged.GameId, gameStateChanged.CorrelationId);
            _bus.Publish(nameof(CommonGameStateChanged),commonGameStateChanged);
        }

        private async Task<CommonGameStateChanged> GetCommonGameStateChanged(Guid gameId, Guid correlationId)
        {
            //todo what to do if gameid is not good?
            var gid = GameId.Factory(gameId);
            var game = await _gameRepository.GetById(gid.Value);
            var roundRemainingPlayers = game.Round.PlayerOrder.Select(p => p.Value.ToString()).ToArray();
            var player1 = game.Players.Skip(0).FirstOrDefault();
            var player2 = game.Players.Skip(1).FirstOrDefault();
            var player3 = game.Players.Skip(2).FirstOrDefault();
            var player4 = game.Players.Skip(3).FirstOrDefault();
            var commonGameStateChanged = new CommonGameStateChanged
            {
                Round = game.Round.Id,
                Discard = game.Round.Discard.Select(cid => $"{(int)cid.CardValue.Value}{cid.Variant}").ToArray(),
                Turn = game.Round.Turn.Id,
                WinningPlayer = game.WinningPlayer?.Value,
                Player1Score = player1?.Score?.Value,
                Player2Score = player2?.Score?.Value,
                Player3Score = player3?.Score?.Value,
                Player4Score = player4?.Score?.Value,
                PlayerOrder = roundRemainingPlayers,
                CorrelationId = correlationId,
                GameId = game.Id.Value,
                DrawCount = game.Round.Deck.Cards.Count(),
            };
            return commonGameStateChanged;
        }
    }
}