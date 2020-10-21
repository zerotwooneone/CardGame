﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Utils.Abstractions.Bus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.Client
{
    public class ClientHub : Hub
    {
        private readonly IHubContext<ClientHub> _context;
        private readonly IBus _bus;
        private readonly ILogger<ClientHub> _logger;

        private static readonly IDictionary<string, ConnectionDetails> ConnectionRepoByConnectionId =
            new ConcurrentDictionary<string, ConnectionDetails>();
        private static readonly IDictionary<Guid, PlayerDetails> ConnectionRepoByPlayerId =
            new ConcurrentDictionary<Guid, PlayerDetails>();

        public ClientHub(IHubContext<ClientHub> context,
            IBus bus,
            ILogger<ClientHub> logger)
        {
            _context = context;
            _bus = bus;
            _logger = logger;
        }

        public async Task<ClientConnected> Connect(ClientIdentifier clientIdentifier)
        {
            //todo: get the player id from authentication
            if (!clientIdentifier.GameId.HasValue)
            {
                _logger.LogError("attempt to connect without a game id");
                return null;
            }

            if (!clientIdentifier.PlayerId.HasValue || clientIdentifier.PlayerId.Value == default)
            {
                _logger.LogError("attempt to connect without a player id");
                return null;
            }

            var playerId = clientIdentifier.PlayerId.Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupIdByGameId(clientIdentifier.GameId.Value));
            ConnectionRepoByConnectionId.Add(Context.ConnectionId,
                new ConnectionDetails(clientIdentifier.GameId.Value, playerId));
            ConnectionRepoByPlayerId.Add(playerId,
                new PlayerDetails(Context.ConnectionId, clientIdentifier.GameId.Value));

            var correlationId = Guid.NewGuid();
            await _bus.Publish(nameof(PlayerConnected),
                new PlayerConnected(clientIdentifier.GameId.Value, playerId, correlationId),
                correlationId: correlationId);

            return new ClientConnected {PlayerId = playerId};
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                _logger.LogError(exception, "client did not choose to disconnect");
            }

            if (ConnectionRepoByConnectionId.TryGetValue(Context.ConnectionId, out var details))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupIdByGameId(details.GameId));
                if (ConnectionRepoByConnectionId.TryGetValue(Context.ConnectionId, out var connectionDetails))
                {
                    ConnectionRepoByPlayerId.Remove(connectionDetails.PlayerId);
                    ConnectionRepoByConnectionId.Remove(Context.ConnectionId);
                }
            }
            else
            {
                //todo: log this
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendClientEvent(ClientEvent clientEvent)
        {
            if (_context.Clients == null)
            {
                _logger.LogWarning($"no clients");
                return;
            }

            // cant use this.Clients
            await _context.Clients.Group(GetGroupIdByGameId(clientEvent.GameId))
                .SendAsync("OnClientEvent", clientEvent);
        }

        private string GetGroupIdByGameId(Guid gameId)
        {
            return $"Game:{gameId}";
        }

        public async Task SendToPlayers(IReadOnlyList<Guid> playerIds, ClientEvent clientEvent)
        {
            var connectionIds = ConvertPlayerIdsToConnectionIds(playerIds);
            if (connectionIds is null || connectionIds.Length == 0)
            {
                _logger.LogWarning($"cannot find players {string.Join(",",playerIds.Select(g => g.ToString()))}");
                return;
            }
            await _context.Clients.Clients(connectionIds).SendAsync("OnClientEvent", clientEvent);
        }

        private string[] ConvertPlayerIdsToConnectionIds(IEnumerable<Guid> playerIds)
        {
            var pids = playerIds as Guid[] ?? playerIds.ToArray();
            if(!pids.Any()) {return new string[0];}
            var result = new List<string>();
            foreach (var playerId in pids)
            {
                if (ConnectionRepoByPlayerId.TryGetValue(playerId, out var playerDetails))
                {
                    result.Add(playerDetails.ConnectionId);
                }
            }

            return result.ToArray();
        }
    }

    public class ClientIdentifier
    {
        public Guid? GameId { get; set; }
        public Guid? PlayerId { get; set; }
    }

    public class ClientConnected
    {
        public Guid PlayerId { get; set; }
    }

    internal class ConnectionDetails
    {
        public Guid GameId { get; }
        public Guid PlayerId { get; }

        public ConnectionDetails(Guid gameId, Guid playerId)
        {
            GameId = gameId;
            PlayerId = playerId;
        }
    }

    internal class PlayerDetails
    {
        public string ConnectionId { get; }
        public Guid GameId { get; }

        public PlayerDetails(string connectionId, Guid gameId)
        {
            ConnectionId = connectionId;
            GameId = gameId;
        }
    }
}