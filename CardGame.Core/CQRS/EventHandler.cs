using System;
using System.Collections.Generic;

namespace CardGame.Core.CQRS
{
    public class EventHandler
    {
        private readonly IDictionary<Type, Func<IEvent, EventResponse>> _eventHandlers;

        public EventHandler(IDictionary<Type, Func<IEvent, EventResponse>> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public EventResponse Handle(IEvent eventObj)
        {
            var type = eventObj.GetType();
            return _eventHandlers[type](eventObj);
        }

        public void AddHandler<T>(Func<IEvent, EventResponse> handler)
        {
            AddHandler(typeof(T), handler);
        }

        public void AddHandler(Type type, Func<IEvent, EventResponse> handler)
        {
            _eventHandlers.Add(type, handler);
        }
    }
}