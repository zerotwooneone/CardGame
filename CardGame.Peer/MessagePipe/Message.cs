using System;
using ProtoBuf;

namespace CardGame.Peer.MessagePipe
{
    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }
        [ProtoMember(2)]
        public string EventJson { get; set; }
        [ProtoMember(3)]
        public bool? SenderNotWaitingForResponse { get; set; }
        [ProtoMember(4)]
        public Response Response { get; set; }
    }
}