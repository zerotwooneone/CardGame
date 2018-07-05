using System.Threading.Tasks;
using CardGame.Peer.MessagePipe;

namespace CardGame.Peer.ResponsePipe
{
    public interface IResponsePipe
    {
        Task<Response> GetResponse(Message message);
        Task SendMessage(Message message);
    }
}