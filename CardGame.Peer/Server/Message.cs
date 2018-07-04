using System;
using ProtoBuf;

namespace CardGame.Peer.Server
{
    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }
        [ProtoMember(2)]
        public string EventJson { get; set; }
    }
}