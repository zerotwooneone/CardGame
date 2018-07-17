using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace CardGame.Core.CQRS
{
    public class EventBroadcaster  
    {
        private readonly EventHandler _eventHandler;
        private readonly ISubject<IEvent> _broadcastSubject;
        public IObservable<IEvent> BroadcastObservable => _broadcastSubject;

        public EventBroadcaster(EventHandler eventHandler, ISubject<IEvent> broadcastSubject)
        {
            _eventHandler = eventHandler;
            _broadcastSubject = broadcastSubject;
        }

        public Task<EventResponse> Broadcast(IEvent eventObj)
        {
            _broadcastSubject.OnNext(eventObj);
            return _eventHandler.Handle(eventObj);
        }
    }
}