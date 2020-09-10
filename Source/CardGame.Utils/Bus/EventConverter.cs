using System;
using System.Collections.Generic;
using CardGame.CommonModel.Bus;
using CardGame.Utils.Abstractions.Bus;
using Newtonsoft.Json;

namespace CardGame.Utils.Bus
{
    public class EventConverter : IEventConverter
    {
        private static readonly IDictionary<string, Registration> Registry = new Dictionary<string,Registration>
        {
            {"RoundStarted", new JsonRegistration<RoundStarted>()},
            {"ServiceCall", new JsonRegistration<ServiceCall>()},
            {"CardPlayed", new JsonRegistration<PlayResponse>()},
        };

        public IReadOnlyDictionary<string, string> GetValues(string topic, object obj)
        {
            if(!Registry.ContainsKey(topic)) throw new ArgumentException($"Unknown Topic: ${topic}", nameof(topic));
            var registration = Registry[topic];
            var eventType = obj.GetType();
            if(eventType != registration.EventType) throw new ArgumentException($"Wrong Event Type \"${eventType}\" for topic ${topic}");
            return registration.GetValues(obj);
        }

        public T GetObject<T>(string topic, 
            IReadOnlyDictionary<string, string> commonEventValues, 
            Guid eventId,
            Guid? correlationId)
        {
            if(!Registry.ContainsKey(topic)) throw new ArgumentException($"Unknown Topic: ${topic}", nameof(topic));
            var registration = Registry[topic];
            var eventType = typeof(T);
            if(eventType != registration.EventType) throw new ArgumentException($"Wrong Event Type \"${eventType}\" for topic ${topic}");
            return registration.GetType<T>(commonEventValues, eventId, correlationId);
        }

        internal class Registration
        {
            public Type EventType { get; set; }

            public Func<object, IReadOnlyDictionary<string, string>> GetValues { get; set; }
            public Func<IReadOnlyDictionary<string, string>, Guid, Guid?, object> GetObject { get; set; }
        }

        internal class JsonRegistration<T>: Registration
        {
            public JsonRegistration()
            {
                EventType = typeof(T);
                GetValues = InnerGetValues;
                GetObject = InnerGetObject;
            }

            private const string JsonKey = "JSON";
            private object InnerGetObject(IReadOnlyDictionary<string, string> values, Guid eventId, Guid? correlationId)
            {
                var obj = JsonConvert.DeserializeObject(values[JsonKey], JsonSerializerSettings);
                return obj;
            }

            private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            }; //todo: make typenamehandling dependent upon isDevelopment
            private IReadOnlyDictionary<string, string> InnerGetValues(object arg)
            {
                return new Dictionary<string, string>
                {
                    {JsonKey, JsonConvert.SerializeObject((T)arg, JsonSerializerSettings) }
                };
            }
        }
        
    }

    public static class RegistrationExtensions
    {
        internal static T GetType<T>(this EventConverter.Registration registration,
            IReadOnlyDictionary<string, string> values,
            Guid eventId,
            Guid? correlationId)
        {
            return (T)registration.GetObject(values, eventId, correlationId);
        }
    }
}