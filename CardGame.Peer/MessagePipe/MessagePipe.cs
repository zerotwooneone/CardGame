using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace CardGame.Peer.MessagePipe
{
    public class MessagePipe
    {
        private readonly IMessagePipe _messagePipe;
        private const double ResponseTimeoutSeconds = 60;
        private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1);

        public MessagePipe(IMessagePipe messagePipe)
        {
            _messagePipe = messagePipe;
        }
        public async Task<Response> GetResponse(Message message)
        {
            var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, message);
            memoryStream.Position = 0;
            Task<Response> responseTask;
            const bool senderNotWaitingForResponseDefault = false;


            await _writeSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (message.SenderNotWaitingForResponse ?? senderNotWaitingForResponseDefault)
                {
                    responseTask = Task.FromResult((Response)null);
                }
                else
                {
                    var waitForResponse = true;
                    responseTask = _messagePipe.MessageObservable
                        .Timeout(TimeSpan.FromSeconds(ResponseTimeoutSeconds))
                        .TakeWhile(b => waitForResponse)
                        .Where(responseMessage => message.Response != null && message.Id == responseMessage.Id)
                        .Select(m => m.Response)
                        .Take(1)
                        .ToTask();
                }
                await _messagePipe.SendMessage(message).ConfigureAwait(false);
                if (message.SenderNotWaitingForResponse ?? senderNotWaitingForResponseDefault)
                {
                    return (Response)null;
                }
                else
                {
                    return await responseTask.ConfigureAwait(false);
                }
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