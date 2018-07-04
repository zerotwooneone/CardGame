using System;
using ProtoBuf;

namespace CardGame.Peer.MessagePipe
{
    [ProtoContract]
    public class Response
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }
        [ProtoMember(2)]
        public bool? Success { get; set; }
        [ProtoMember(3)]
        public string CreatedId { get;set; }
    }
}