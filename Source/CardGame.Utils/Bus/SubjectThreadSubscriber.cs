using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Utils.Bus
{
    /// <summary>
    /// All topics which use the same instance will subscribe on the same thread. Any bad actor which takes a long time to handle the event will block all other subscribers
    /// </summary>
    public class SubjectThreadSubscriber : IQueueStrategy
    {
        private readonly ISubject<ICommonEvent> _publishSubject;
        private readonly IScheduler _scheduler;

        public SubjectThreadSubscriber(ISubject<ICommonEvent> publishSubject)
        {
            _publishSubject = publishSubject;
            _scheduler = new EventLoopScheduler(ts => new Thread(ts) {IsBackground = true}); 
            //_scheduler = new TaskPoolScheduler(new TaskFactory()); // will create a thread for each subscriber
        }
        public Task Publish(ICommonEvent commonEvent)
        {
            _publishSubject.OnNext(commonEvent);
            return Task.CompletedTask;
        }

        public ISubscription Subscribe(string topic, Func<ICommonEvent, Task> handler)
        {
            var subscription = _publishSubject 
                .ObserveOn(_scheduler)
                .Where(ce => ce.Topic == topic)
                .SelectMany(c => handler(c).ContinueWith(t => Unit.Default)) //we explicitly ignore the return of the handler value here
                .Subscribe();
            var result = new SubscriptionWrapper(subscription);
            return result;
        }
    }
}