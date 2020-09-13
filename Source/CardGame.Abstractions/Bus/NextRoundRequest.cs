using System;

namespace CardGame.CommonModel.Bus
{
    public class NextRoundRequest: IRequest
    {
        public Guid GameId { get; set; }
        public Guid WinningPlayer { get; set; }
        public Guid CorrelationId { get; set; }
    }
}