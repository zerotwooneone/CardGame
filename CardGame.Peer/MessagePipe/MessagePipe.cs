using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using ProtoBuf;

namespace CardGame.Peer.MessagePipe
{
    public class MessagePipe
    {
        private readonly IMessagePipe _messagePipe;
        private const double ResponseTimeoutSeconds = 60;

        public MessagePipe(IMessagePipe messagePipe)
        {
            _messagePipe = messagePipe;
        }
        public async Task<Response> GetResponse(Message message)
        {
            var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, message);
            memoryStream.Position = 0;
            const bool senderNotWaitingForResponseDefault = false;

            Task<Response> responseTask;
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

        public async Task SendMessage(Message message)
        {
            await _messagePipe.SendMessage(message).ConfigureAwait(false);
        }
    }
}