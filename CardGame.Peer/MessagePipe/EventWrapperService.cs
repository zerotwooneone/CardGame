using System;
using System.Collections.Generic;
using CardGame.Core.CQRS;
using Newtonsoft.Json;

namespace CardGame.Peer.MessagePipe
{
    public class EventWrapperService
    {
        public EventWrapper Wrap(IEvent eventObj)
        {
            var type = eventObj.GetType();
            var hashCode = GetTypeHashCode(type);
            var eventWrapper = new EventWrapper
            {
                Json = JsonConvert.SerializeObject(eventObj),
                Type = hashCode
            };
            return eventWrapper;
        }

        private static string GetTypeHashCode(Type type)
        {
            return type.ToString();
        }

        public IEvent UnWrap(EventWrapper eventWrapper)
        {
            Type type = _types[eventWrapper.Type];
            var obj = JsonConvert.DeserializeObject(eventWrapper.Json, type);
            var eventObj = (IEvent)obj;
            return eventObj;
        }

        private static readonly IDictionary<string, Type> _types = new Dictionary<string, Type>();

        public void AddEventType<T>() where T : IEvent
        {
            AddEventType(typeof(T));
        }

        public void AddEventType(Type type)
        {
            var hashCode = GetTypeHashCode(type);
            _types.Add(hashCode, type);
        }
    }
}