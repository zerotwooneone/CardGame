using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Newtonsoft.Json;

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
                if (handlerConfig.Handler != null)
                {
                    var subscription = _messagePipe
                        .MessageObservable
                        //.Select(m=>
                        //{
                        //    Console.WriteLine($"trying to handle message:{JsonConvert.SerializeObject(m)}");
                        //    return m;
                        //})
                        .Where(m => handlerConfig.Filter(m))
                        .Subscribe(m => { handlerConfig.Handler(m); });
                    _subscriptions.Add(subscription);
                }
            }

            var s = _messagePipe
                .MessageObservable
                .Where(m =>
                {
                    return m.Response == null;
                })
                .Subscribe(m =>
                {
                    var handler = responseHandlers.FirstOrDefault(kvp => kvp.Key(m));
                    if (default(KeyValuePair<Func<Message, bool>, Func<Message, Response>>).Equals(handler)) return;
                    var response = responseHandlers[handler.Key](m);
                    _messagePipe.SendMessage(new Message { Id = m.Id, Response = response });
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
                    Console.WriteLine(e);
                }
            }
            _messagePipe?.Dispose();
        }
    }
}
