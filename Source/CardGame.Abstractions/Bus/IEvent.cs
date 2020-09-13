using System;

namespace CardGame.CommonModel.Bus
{
    public interface IEvent
    {
        Guid CorrelationId { get; }
    }
}