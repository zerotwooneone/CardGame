using System;

namespace CardGame.Utils.Entity
{
    public abstract class Entity<T>
    {
        public T Id { get; }

        protected Entity(T id)
        {
            if(id.Equals(default(T))) throw new ArgumentException("id required", nameof(id));
            Id = id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Entity<T>;
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }
    }
}
