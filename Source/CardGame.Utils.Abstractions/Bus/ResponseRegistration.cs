using System;

namespace CardGame.Utils.Abstractions.Bus
{
    public class ResponseRegistration
    {
        public Type ServiceType { get; }
        public Func<object> Resolve { get; }
        public string Method { get; }
        public string ResponseTopic { get; }

        public ResponseRegistration(Type serviceType, Func<object> resolve, string method, string responseTopic)
        {
            ServiceType = serviceType;
            Resolve = resolve;
            Method = method;
            ResponseTopic = responseTopic;
        }
    }
}