using System;

namespace CardGame.Utils.Abstractions.Bus
{
    public class RequestConfiguration
    {
        public string Service { get; }
        public string Method { get; }
        public string ResponseTopic { get; }

        public RequestConfiguration(string service,
            string method,
            string responseTopic)
        {
            if(string.IsNullOrWhiteSpace(service)) throw new ArgumentNullException(nameof(service));
            if(string.IsNullOrWhiteSpace(method)) throw new ArgumentNullException(nameof(method));
            Service = service;
            Method = method;
            ResponseTopic = responseTopic;
        }
    }
}