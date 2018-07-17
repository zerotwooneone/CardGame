using System;
using CardGame.Core.CQRS;

namespace CardGame.Core.Game
{
    public class GameFactory
    {
        private readonly EventBroadcaster _eventBroadcaster;

        public GameFactory(EventBroadcaster eventBroadcaster)
        {
            _eventBroadcaster = eventBroadcaster;
        }

        public Game CreateNew()
        {
            return new Game(_eventBroadcaster);
        }

        public Game GetFromId(Guid id)
        {
            return new Game(_eventBroadcaster, id, null);
        }
    }
}