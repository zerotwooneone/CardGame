using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Utils.Abstractions.Bus;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Application.Client
{
    public class ClientHub : Hub
    {
        private readonly IHubContext<ClientHub> _context;
        private readonly IBus _bus;

        private static readonly IDictionary<string, ConnectionDetails> _connectionRepo =
            new ConcurrentDictionary<string, ConnectionDetails>();

        public ClientHub(IHubContext<ClientHub> context,
            IBus bus)
        {
            _context = context;
            _bus = bus;
        }

        public async Task<ClientConnected> Connect(ClientIdentifier clientIdentifier)
        {
            //todo: get the player id from authentication
            Guid playerId = Guid.NewGuid();
            if (clientIdentifier.GameId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupIdByGameId(clientIdentifier.GameId.Value));
                _connectionRepo.Add(Context.ConnectionId,
                    new ConnectionDetails(clientIdentifier.GameId.Value, playerId));
            }

            return new ClientConnected {PlayerId = playerId};
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                //todo: log this. this means the caller did not choose to quit
            }

            if (_connectionRepo.TryGetValue(Context.ConnectionId, out var details))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupIdByGameId(details.GameId));
            }
            else
            {
                //todo: log this
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendClientEvent(ClientEvent clientEvent)
        {
            if (_context.Clients != null)
            {
                // cant use this.Clients
                await _context.Clients.Group(GetGroupIdByGameId(clientEvent.GameId))
                    .SendAsync("OnClientEvent", clientEvent);
            }
        }

        private string GetGroupIdByGameId(Guid gameId)
        {
            return $"Game:{gameId}";
        }
    }

    public class ClientIdentifier
    {
        public Guid? GameId { get; set; }
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
}