using System;

namespace CardGame.CommonModel.Bus
{
    public class ServiceCallFailed: IEvent
    {
        public object Param { get; set; }
        public Guid EventId { get; set; }
        public Guid CorrelationId { get; set; }
        public string RequestTopic { get; set; }
        public string Exception { get; set; }
        public Guid ServiceCallEventId { get; set; }
        public string Method { get; set; }
        public string Service { get; set; }
        public static ServiceCallFailed Factory(ServiceCall sc, Exception exception, string service = default, string method = default)
        {
            return new ServiceCallFailed
            {
                CorrelationId = sc.CorrelationId,
                Method = method,
                Param = sc.Param,
                Service = service,
                Exception = exception.ToString(),
                ServiceCallEventId = sc.EventId,
            };
        }

        
    }
}