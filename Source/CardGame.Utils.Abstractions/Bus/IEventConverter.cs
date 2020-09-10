using System;
using System.Collections.Generic;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface IEventConverter
    {
        IReadOnlyDictionary<string, string> GetValues(string topic, object obj);
        T GetObject<T>(string topic, 
            IReadOnlyDictionary<string, string> commonEventValues, 
            Guid eventId, 
            Guid? correlationId);
    }
}