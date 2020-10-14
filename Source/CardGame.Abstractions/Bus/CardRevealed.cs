using System;

namespace CardGame.CommonModel.Bus
{
    public class CardRevealed : IEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
        public int TargetCardStrength { get; set; }
        public int TargetCardVariant { get; set; }
        public Guid TargetId { get; set; }
    }
}