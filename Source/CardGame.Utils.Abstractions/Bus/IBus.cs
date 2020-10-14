using System;
using System.Threading;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;

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
        Task Publish(string topic, object value, Guid correlationId = default, Guid eventId = default);

        /// <summary>
        /// Make a request and await the first response
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="requestTopic"></param>
        /// <param name="value"></param>
        /// <param name="correlationId">the id that related the request and the response events</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> Request<TResponse>(string requestTopic,
            object value, 
            Guid correlationId, 
            Guid eventId = default,
            CancellationToken cancellationToken = default);

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

    public static class BusExtensions
    {
        /// <summary>
        /// Publishes a value to all listeners of a topic
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="topic"></param>
        /// <param name="value"></param>
        /// <param name="eventId">Optional: this event's id</param>
        public static async Task PublishEvent<T>(this IBus bus, 
            string topic, 
            T value,
            Guid eventId = default) where T : IEvent
        {
            await bus.Publish(topic, value, value.CorrelationId, eventId);
        }

        /// <summary>
        /// Make a request and await the first response
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="bus"></param>
        /// <param name="requestTopic"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TResponse> Request<TRequest, TResponse>(this IBus bus, 
                string requestTopic,
                TRequest value, 
                Guid eventId = default,
                CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            return await bus.Request<TResponse>(requestTopic, value, value.CorrelationId, eventId, cancellationToken);
        }

        /// <summary>
        /// Subscribes to the topic, but unsubscribes after handling the first message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bus"></param>
        /// <param name="topic"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static ISubscription SubscribeToFirst<T>(this IBus bus,
            string topic,
            Func<T, Task> handler)
        {
            ISubscription subscription = null;
            async Task HandleFirst(T m)
            {
                await handler(m);
                subscription?.Dispose();
            }
            subscription = bus.Subscribe<T>(topic, HandleFirst);
            return subscription;
        }
    }
}