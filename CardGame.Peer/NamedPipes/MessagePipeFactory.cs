using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CardGame.Peer.MessagePipe;

namespace CardGame.Peer.NamedPipes
{
    public class MessagePipeFactory
    {
        private readonly Func<NamedPipeConfig, ClientPipe> _clientPipeFactory;
        private readonly Func<NamedPipeConfig, ServerPipe> _serverPipeFactory;
        public static readonly TimeSpan ClientConnecTimeSpan = TimeSpan.FromSeconds(1.0);
        public MessagePipeFactory(Func<NamedPipeConfig,ClientPipe> clientPipeFactory,
            Func<NamedPipeConfig,ServerPipe> serverPipeFactory)
        {
            _clientPipeFactory = clientPipeFactory;
            _serverPipeFactory = serverPipeFactory;
        }
        
        public async Task<IMessagePipe> GetMessagePipe(NamedPipeConfig namedPipeConfig)
        {
            if (string.IsNullOrWhiteSpace(namedPipeConfig.PipeName))
            {
                throw new ArgumentException("The name of the pipe cannot be null or blank");
            }
            IMessagePipe result;
            try
            {
                var clientPipe = _clientPipeFactory(namedPipeConfig);
                await Task.Run(() =>
                {
                    clientPipe.Connect(ClientConnecTimeSpan);
                }).ConfigureAwait(false);
                result = new MessageClient(clientPipe);
            }
            catch (TimeoutException e) when (e.Message.StartsWith("The operation has timed out."))
            {
                if (string.IsNullOrWhiteSpace(namedPipeConfig.ServerName))
                {
                    throw new ArgumentException("Could not connect as a client, and the server name is not set.", nameof(namedPipeConfig));
                }
                var serverPipe = _serverPipeFactory(namedPipeConfig);
                await serverPipe.ConnectedObservable
                    .Where(c => c)
                    .Take(1)
                    .ToTask().ConfigureAwait(false);
                result = new MessageServer(serverPipe);
            }
            return result;
        }
    }
}
