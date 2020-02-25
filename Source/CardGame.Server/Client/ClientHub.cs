using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Server.Client
{
    public class ClientHub : Hub
    {
        private readonly IHubContext<ClientHub> _context;
        public ClientHub(IHubContext<ClientHub> context)
        {
            _context = context;
        }

        public async Task<IClientConnected> connect(ClientIdentifier clientIdentifier)
        {
            return new ClientConnected { X = DateTime.Now.ToString("yyyyMMdd-HHmmss.ffff") };
        }

        //public async Task<IClientConnected> Connect(IClientIdentifier clientIdentifier)
        //{
        //    return new ClientConnected{ X= DateTime.Now.ToString("yyyyMMdd-HHmmss.ffff")};
        //}
    }

    public class ClientIdentifier
    {
        private string Id { get; set; }
    }
}