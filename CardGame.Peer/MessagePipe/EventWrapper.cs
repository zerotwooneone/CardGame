using ProtoBuf;

namespace CardGame.Peer.MessagePipe
{
    [ProtoContract]
    public class EventWrapper
    {
        [ProtoMember(1)]
        public string Type { get; set; }
        [ProtoMember(2)]
        public string Json { get; set; }
    }
}