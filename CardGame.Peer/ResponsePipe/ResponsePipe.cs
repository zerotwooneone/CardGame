using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using CardGame.Peer.MessagePipe;
using CardGame.Peer.ResponsePipe;

namespace CardGame.Peer.NamedPipes
{
    public class ResponsePipe : IResponsePipe
    {
        private readonly IMessagePipe _messagePipe;
        private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1);

        public ResponsePipe(IMessagePipe messagePipe)
        {
            _messagePipe = messagePipe;
        }
        public async Task<Response> GetResponse(Message message)
        {
            var responseTask = _messagePipe.MessageObservable
                .Where(m =>
                {
                    return m.Response != null && m.Id == message.Id;
                })
                .Take(1)
                .Select(m =>
                {
                    return m.Response;
                })
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
