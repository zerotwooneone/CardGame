using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Utils.Abstractions.Bus;
using Newtonsoft.Json;

namespace CardGame.Utils.Bus
{
    public class EventConverter : IEventConverter
    {
        //todo: move this to startup and config file
        private static readonly IDictionary<string, Registration> Registry = new Dictionary<string,Registration>
        {
            {"RoundStarted", new JsonRegistration("CardGame.CommonModel.Bus.RoundStarted")},
            {"ServiceCall", new JsonRegistration("CardGame.CommonModel.Bus.ServiceCall")},
            {"ServiceCallFailed", new JsonRegistration("CardGame.CommonModel.Bus.ServiceCallFailed")},
            {"CardPlayed", new JsonRegistration("CardGame.CommonModel.Bus.CardPlayed")},
            {"TurnChanged", new JsonRegistration("CardGame.CommonModel.Bus.TurnChanged") },
            {nameof(GameStateChanged), new JsonRegistration("CardGame.CommonModel.Bus.GameStateChanged") },
            {nameof(CommonGameStateChanged), new JsonRegistration("CardGame.CommonModel.Bus.CommonGameStateChanged") },
            {nameof(PlayerConnected), new JsonRegistration("CardGame.CommonModel.Bus.PlayerConnected") },
            {nameof(Rejected), new JsonRegistration(typeof(Rejected).ToString()) },
            {nameof(CardRevealed), new JsonRegistration(typeof(CardRevealed).ToString()) }
        };

        public IReadOnlyDictionary<string, string> GetValues(string topic, object obj)
        {
            if(!Registry.ContainsKey(topic)) throw new ArgumentException($"Unknown Topic: ${topic}", nameof(topic));
            var registration = Registry[topic];
            var eventType = obj.GetType();
            if(eventType.ToString() != registration.EventType) throw new ArgumentException($"Wrong Event Type \"${eventType}\" for topic ${topic}");
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
            if(eventType.ToString() != registration.EventType) throw new ArgumentException($"Wrong Event Type \"${eventType}\" for topic ${topic}");
            return registration.GetType<T>(commonEventValues, eventId, correlationId);
        }

        public bool CanConvert(string topic)
        {
            return Registry.ContainsKey(topic);
        }

        public async Task Publish(string topic, ICommonEvent commonEvent)
        {
            if(!Registry.TryGetValue(topic, out var registration)) throw new ArgumentException($"Unknown Topic: ${topic}", nameof(topic));
            await registration.QueueStrategy.Publish(commonEvent);
        }

        public ISubscription Subscribe(string topic, Func<ICommonEvent, Task> handler)
        {
            if(!Registry.TryGetValue(topic, out var registration)) throw new ArgumentException($"Unknown Topic: ${topic}", nameof(topic));
            return registration.QueueStrategy.Subscribe(topic, handler);
        }

        internal class Registration
        {
            public Registration(string eventType, 
                Func<object, IReadOnlyDictionary<string, string>> getValues, 
                Func<IReadOnlyDictionary<string, string>, Guid, Guid?, object> getObject,
                IQueueStrategy queueStrategy)
            {
                EventType = eventType;
                GetValues = getValues;
                GetObject = getObject;
                QueueStrategy = queueStrategy;
            }

            public string EventType { get; }

            public Func<object, IReadOnlyDictionary<string, string>> GetValues { get; }
            public Func<IReadOnlyDictionary<string, string>, Guid, Guid?, object> GetObject { get; }
            public IQueueStrategy QueueStrategy { get; }
        }

        internal class JsonRegistration: Registration
        {
            //todo: remove static init, create strategy factory
            private static readonly IQueueStrategy CommonStrategy = new SubjectThreadSubscriber(new Subject<ICommonEvent>());
            public JsonRegistration(string type) : base(type, InnerGetValues, InnerGetObject, CommonStrategy)
            {
            }

            private const string JsonKey = "JSON";
            private static object InnerGetObject(IReadOnlyDictionary<string, string> values, Guid eventId, Guid? correlationId)
            {
                var obj = JsonConvert.DeserializeObject(values[JsonKey], JsonSerializerSettings);
                return obj;
            }

            private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            }; //todo: make typenamehandling dependent upon isDevelopment
            private static IReadOnlyDictionary<string, string> InnerGetValues(object arg)
            {
                return new Dictionary<string, string>
                {
                    {JsonKey, JsonConvert.SerializeObject(arg, JsonSerializerSettings) }
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