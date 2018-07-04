using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CardGame.Peer.NamedPipes;
using ProtoBuf;

namespace CardGame.Peer.MessagePipe
{
    public class MessageServer: IMessagePipe
    {
        private readonly ServerPipe _serverPipe;
        //public IObservable<int> ClientConnectedObservable { get; }
        private readonly ISubject<Message> _messageSubject;
        public IObservable<Message> MessageObservable { get; }

        public MessageServer(ServerPipe serverPipe)
        {
            _messageSubject = new Subject<Message>();
            MessageObservable = _messageSubject;
            _serverPipe = serverPipe;
            _serverPipe.DataReadObservable.Subscribe(_serverPipe_ReadDataEvent);
            //ClientConnectedObservable = Observable.FromEvent<EventHandler<EventArgs>, int>(
            //    onNextHandler =>
            //    {
            //        void HandleEvent(object x, EventArgs y)
            //        {
            //            onNextHandler(0);
            //        }
            //        return HandleEvent;
            //    },
            //    h => _serverPipe.GotConnectionEvent += h,
            //    h => _serverPipe.GotConnectionEvent -= h);
        }

        private void _serverPipe_ReadDataEvent(byte[] bytes)
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
            return _serverPipe.WriteByteArray(memoryStream.ToArray());
        }

        public void Dispose()
        {
            _serverPipe?.Dispose();
        }
    }
}