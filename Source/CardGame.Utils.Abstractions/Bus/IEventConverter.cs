using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface IEventConverter
    {
        IReadOnlyDictionary<string, string> GetValues(string topic, object obj);
        T GetObject<T>(string topic, 
            IReadOnlyDictionary<string, string> commonEventValues, 
            Guid eventId, 
            Guid? correlationId);

        bool CanConvert(string topic);

        Task Publish(string topic, ICommonEvent commonEvent);
        ISubscription Subscribe(string topic, Func<ICommonEvent, Task> handler);
    }
}