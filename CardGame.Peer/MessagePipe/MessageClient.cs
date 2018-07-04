using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CardGame.Peer.NamedPipes;
using ProtoBuf;

namespace CardGame.Peer.MessagePipe
{
    public class MessageClient : IMessagePipe
    {
        
        private readonly ClientPipe _clientPipe;
        private readonly ISubject<Message> _messageSubject;

        public IObservable<Message> MessageObservable { get; }

        public MessageClient(ClientPipe clientPipe)
        {
            _messageSubject = new Subject<Message>();
            MessageObservable = _messageSubject;
            _clientPipe = clientPipe;
            _clientPipe.DataReadObservable.Subscribe(_clientPipe_ReadDataEvent);
        }

        private void _clientPipe_ReadDataEvent(byte[] bytes)
        {
            var memoryStream = new MemoryStream(bytes, 0, bytes.Length);
            var message = Serializer.Deserialize<Message>(memoryStream);
            _messageSubject.OnNext(message);
        }

        public Task SendMessage(Message message)
        {
            var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, message);
            memoryStream.Position = 0;
            return _clientPipe.WriteByteArray(memoryStream.ToArray());
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