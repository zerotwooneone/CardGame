using System;

namespace CardGame.CommonModel.Bus
{
    public class ServiceCallFailed : ServiceCall
    {
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