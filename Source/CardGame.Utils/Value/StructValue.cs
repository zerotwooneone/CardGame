namespace CardGame.Utils.Value
{
    public abstract class StructValue<T> : Value where T : struct
    {
        public T Value { get; }

        protected StructValue(T value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as StructValue<T>;
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Value.Equals(other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}