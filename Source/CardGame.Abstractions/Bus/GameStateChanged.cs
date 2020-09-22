using System;

namespace CardGame.CommonModel.Bus
{
    public class GameStateChanged : IEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid GameId { get; set; }
    }
}

