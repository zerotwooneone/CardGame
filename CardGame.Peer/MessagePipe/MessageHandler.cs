using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace CardGame.Peer.MessagePipe
{
    public class MessageHandler : IDisposable
    {
        private readonly IMessagePipe _messagePipe;
        private readonly IList<IDisposable> _subscriptions;

        public MessageHandler(IMessagePipe messagePipe)
        {
            _messagePipe = messagePipe;
            _subscriptions = new List<IDisposable>();
        }

        public void RegisterHandlers(IEnumerable<HandlerConfig> handlerConfigs, IReadOnlyDictionary<Func<Message, bool>, Func<Message, Response>> responseHandlers)
        {
            foreach (var handlerConfig in handlerConfigs)
            {
                var subscription = _messagePipe
                    .MessageObservable
                    .Select(m =>
                    {
                        return m;
                    })
                    .Where(handlerConfig.Filter)
                    .Select(m =>
                    {
                        return m;
                    })
                    .Subscribe(handlerConfig.Handler);
                _subscriptions.Add(subscription);
            }

            var s = _messagePipe
                .MessageObservable
                .Subscribe(m =>
                {
                    var handler = responseHandlers.FirstOrDefault(kvp => kvp.Key(m));
                    if (!default(KeyValuePair<Func<Message, bool>, Func<Message, Response>>).Equals(handler))
                    {
                        var response = responseHandlers[handler.Key](m);
                        _messagePipe.SendMessage(new Message { Id = m.Id, Response = response });
                    }
                });
            _subscriptions.Add(s);
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                try
                {
                    subscription.Dispose();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            _messagePipe?.Dispose();
        }
    }

    public class HandlerConfig
    {
        public Func<Message, bool> Filter { get; set; }
        public Action<Message> Handler { get; set; }
    }
}
