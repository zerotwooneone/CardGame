using System;

namespace CardGame.CommonModel.Bus
{
    public class ServiceCallFailed : ServiceCall
    {
        public static ServiceCallFailed Factory(ServiceCall sc, Exception exception)
        {
            return new ServiceCallFailed
            {
                CorrelationId = sc.CorrelationId,
                Method = sc.Method,
                Param = sc.Param,
                Service = sc.Service,
                Exception = exception.ToString(),
                ServiceCallEventId = sc.EventId,
            };
        }

        public string Exception { get; set; }
        public Guid ServiceCallEventId { get; set; }
    }
}