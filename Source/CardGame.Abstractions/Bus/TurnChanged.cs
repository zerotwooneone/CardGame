using System;

namespace CardGame.CommonModel.Bus
{
    public class TurnChanged : IEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid? NextRoundFirstPlayer { get; set; }
        public int? NextTurnId { get; set; }
    }
}