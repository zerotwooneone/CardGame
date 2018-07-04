using System;
using System.Threading.Tasks;

namespace CardGame.Peer.MessagePipe
{
    public interface IMessagePipe
    {
        Task<Response> GetResponse(Message message);
        Task SendMessage(Message message);
        IObservable<Message> MessageObservable { get; }
    }
}