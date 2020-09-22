using System;
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

        public ClientHub(IHubContext<ClientHub> context,
            IBus bus)
        {
            _context = context;
            _bus = bus;
        }

        public async Task<ClientConnected> Connect(ClientIdentifier clientIdentifier)
        {
            //todo: use gameid to group connections together
            return new ClientConnected { PlayerId = Guid.NewGuid().ToString()};
        }

        public async Task SendClientEvent(ClientEvent clientEvent)
        {
            if (_context.Clients != null)
            {
                //await Clients.All.SendAsync("OnClientEvent", clientEvent);
                await _context.Clients.All.SendAsync("OnClientEvent", clientEvent);
            }
        }
    }

    public class ClientIdentifier
    {
        public Guid? GameId { get; set; }
    }

    public class ClientConnected
    {
        public string PlayerId { get; set; }
    }
}