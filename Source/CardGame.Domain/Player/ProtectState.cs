using CardGame.Utils.Value;

namespace CardGame.Domain.Player
{
    public class ProtectState : StructValue<bool>
    {
        public static readonly ProtectState Protected = new ProtectState(true);
        public static readonly ProtectState UnProtected = new ProtectState(false);
        protected ProtectState(bool value) : base(value)
        {
        }
    }
}