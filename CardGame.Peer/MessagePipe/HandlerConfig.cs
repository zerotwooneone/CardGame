using System;

namespace CardGame.Peer.MessagePipe
{
    public class HandlerConfig
    {
        public Func<Message, bool> Filter { get; set; }
        public Action<Message> Handler { get; set; }
        //public Func<Message, Response> ResponseHandler { get; set; }
    }
}