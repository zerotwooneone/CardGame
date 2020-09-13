using System;

namespace CardGame.CommonModel.Bus
{
    public interface IRequest
    {
        Guid CorrelationId { get; }
    }
}