using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using CardGame.Peer.MessagePipe;

namespace CardGame.Peer.ResponsePipe
{
    public class ResponsePipe : IResponsePipe
    {
        private readonly IMessagePipe _messagePipe;
        private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(30);

        public ResponsePipe(IMessagePipe messagePipe)
        {
            _messagePipe = messagePipe;
        }
        public async Task<Response> GetResponse(Message message)
        {
            var responseTask = _messagePipe.MessageObservable
                .Where(m => m.Response != null && m.Id == message.Id)
                .Take(1)
                .Timeout(ResponseTimeout)
                .Select(m => m.Response)
                .ToTask();
            await _writeSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await _messagePipe.SendMessage(message).ConfigureAwait(false);
                return await responseTask.ConfigureAwait(false);
            }
            finally
            {
                _writeSemaphore.Release();
            }
        }

        public async Task SendMessage(Message message)
        {
            await _writeSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await _messagePipe.SendMessage(message).ConfigureAwait(false);
            }
            finally
            {
                _writeSemaphore.Release();
            }
        }
    }
}
