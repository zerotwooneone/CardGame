using System;
using System.Threading.Tasks;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface IQueueStrategy
    {
        Task Publish(ICommonEvent commonEvent);
        ISubscription Subscribe(string topic, Func<ICommonEvent, Task> handler);
    }
}