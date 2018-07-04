using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CardGame.Peer.NamedPipes;
using ProtoBuf;

namespace CardGame.Peer.MessagePipe
{
    public class MessageClient : IMessagePipe, IDisposable
    {
        private const string PipeServername = ".";
        private readonly ClientPipe _clientPipe;
        private readonly ISubject<Message> _messageSubject;
        private const double ResponseTimeoutSeconds = 60;
        public IObservable<Message> MessageObservable { get; }

        public MessageClient()
        {
            _messageSubject = new Subject<Message>();
            MessageObservable = _messageSubject;
            _clientPipe = new ClientPipe(PipeServername, MessageServer.MytestPipeName);
            _clientPipe.DataReadObservable.Subscribe(_clientPipe_ReadDataEvent);
        }

        private void _clientPipe_ReadDataEvent(byte[] bytes)
        {
            var memoryStream = new MemoryStream(bytes, 0, bytes.Length);
            var message = Serializer.Deserialize<Message>(memoryStream);
            _messageSubject.OnNext(message);
        }

        public Task<Response> GetResponse(Message message)
        {
            var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, message);
            memoryStream.Position = 0;
            Task<Response> responseTask;
            const bool senderNotWaitingForResponseDefault = false;
            if (message.SenderNotWaitingForResponse ?? senderNotWaitingForResponseDefault)
            {
                responseTask = Task.FromResult((Response)null);
            }
            else
            {
                var waitForResponse = true;
                responseTask = _clientPipe.DataReadObservable
                    .Timeout(TimeSpan.FromSeconds(ResponseTimeoutSeconds))
                    .TakeWhile(b => waitForResponse)
                    .Select(b =>
                    {
                        memoryStream = new MemoryStream(b, 0, b.Length);
                        var responseMessage = Serializer.Deserialize<Message>(memoryStream);
                        return responseMessage;
                    })
                    .Where(responseMessage => message.Response != null && message.Id == responseMessage.Id)
                    .Select(m => m.Response)
                    .Take(1)
                    .ToTask();
            }

            var writeTask = _clientPipe.WriteByteArray(memoryStream.ToArray());
            if (message.SenderNotWaitingForResponse ?? senderNotWaitingForResponseDefault)
            {
                return writeTask.ContinueWith(t => (Response)null);
            }
            else
            {
                return responseTask;
            }
        }

        public Task SendMessage(Message message)
        {
            message.SenderNotWaitingForResponse = true;
            return GetResponse(message);
        }

        public void Connect(TimeSpan? timeout = null)
        {
            _clientPipe.Connect(timeout);
        }

        public void Dispose()
        {
            _clientPipe?.Dispose();
        }
    }
}