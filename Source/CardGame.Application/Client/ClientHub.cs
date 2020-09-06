﻿using System;
using System.Threading.Tasks;
using CardGame.CommonModel.Client;
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

        public async Task<IClientConnected> Connect(ClientIdentifier clientIdentifier)
        {
            return new ClientConnected { Id = Guid.NewGuid().ToString()};
        }
    }

    public class ClientIdentifier : IClientIdentifier
    {
        private string Id { get; set; }
    }

    public class ClientConnected : IClientConnected
    {
        public string Id { get; set; }
    }
}