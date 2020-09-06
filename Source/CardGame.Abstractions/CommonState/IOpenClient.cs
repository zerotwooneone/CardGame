using System.Threading;
using System.Threading.Tasks;

namespace CardGame.CommonModel.CommonState
{
    public interface IOpenClient
    {
        Task Call(string method, object param, CancellationToken cancellationToken = default(CancellationToken));
    }
}