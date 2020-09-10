using System;
using System.Threading;
using System.Threading.Tasks;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface IBus
    {
        void Publish<T>(string topic, T value, Guid correlationId = default, Guid eventId = default);
        Task<TResponse> Request<TRequest, TResponse>(string requestTopic,
            Guid correlationId,
            TRequest value, 
            CancellationToken cancellationToken = default);
        ISubscription Subscribe<T>(string topic, Action<T> handler);
    }
}
