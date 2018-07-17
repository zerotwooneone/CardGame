using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardGame.Core.CQRS
{
    public class EventHandler
    {
        private readonly IDictionary<Type, Func<IEvent, Task<EventResponse>>> _eventHandlers;

        public EventHandler(IDictionary<Type, Func<IEvent, Task<EventResponse>>> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public Task<EventResponse> Handle(IEvent eventObj)
        {
            var type = eventObj.GetType();
            return _eventHandlers[type](eventObj);
        }

        public EventHandler AddHandler<T>(Func<IEvent, EventResponse> handler)
        {
            return AddHandler(typeof(T), handler);
        }

        public EventHandler AddHandler(Type type, Func<IEvent, EventResponse> handler)
        {
            _eventHandlers.Add(type, e => Task.FromResult(handler(e)));
            return this;
        }

        public EventHandler AddHandler<T>(Func<IEvent, Task<EventResponse>> handler)
        {
            return AddHandler(typeof(T), handler);
        }

        public EventHandler AddHandler(Type type, Func<IEvent, Task<EventResponse>> handler)
        {
            _eventHandlers.Add(type, handler);
            return this;
        }
    }
}