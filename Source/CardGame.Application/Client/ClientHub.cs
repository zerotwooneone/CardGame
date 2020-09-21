using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Application.Client
{
    public class ClientHub : Hub
    {
        private readonly IHubContext<ClientHub> _context;
        public ClientHub(IHubContext<ClientHub> context)
        {
            _context = context;
        }

        public async Task<ClientConnected> Connect(ClientIdentifier clientIdentifier)
        {
            return new ClientConnected { Id = Guid.NewGuid().ToString()};
        }
    }

    public class ClientIdentifier
    {
        public Guid? GameId { get; set; }
    }

    public class ClientConnected
    {
        public string Id { get; set; }
    }
}