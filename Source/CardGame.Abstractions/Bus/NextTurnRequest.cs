using System;

namespace CardGame.CommonModel.Bus
{
    public class NextTurnRequest: IRequest
    {
        public Guid GameId { get; set; }
        public Guid CorrelationId { get; set; }
    }
}