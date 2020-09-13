using System;
using System.Threading;
using System.Threading.Tasks;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface IBus
    {
        /// <summary>
        /// Publishes a value to all listeners of a topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="value"></param>
        /// <param name="correlationId">Optional: original event id that triggered this publish</param>
        /// <param name="eventId">Optional: this event's id</param>
        void Publish(string topic, object value, Guid correlationId = default, Guid eventId = default);
        
        /// <summary>
        /// Make a request and await the first response
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="requestTopic"></param>
        /// <param name="correlationId"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> Request<TRequest, TResponse>(string requestTopic,
            Guid correlationId,
            TRequest value, 
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Register a handler method which will be called each time a publish is made to the topic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        ISubscription Subscribe<T>(string topic, Action<T> handler);
        /// <summary>
        /// Register a handler method which will be called each time a publish is made to the topic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="handler">async method to be called when a response is received</param>
        /// <returns></returns>
        [Obsolete("need to improve async handling")]
        ISubscription Subscribe<T>(string topic, Func<T, Task> handler);
    }
}