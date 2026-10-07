namespace DientesLimpios.Domain.Common
{
    // Entities are equal when they are the same kind of thing with the same identity,
    // whatever their state: two copies of one appointment loaded in different scopes are
    // the same appointment. An entity without an Id yet (only possible while EF Core is
    // materialising it) has no identity and is equal only to itself.
    public abstract class Entity : IEquatable<Entity>
    {
        public Guid Id { get; private set; }

        protected Entity(Guid id) => Id = id;
        protected Entity() { }  // EF Core

        public static bool operator ==(Entity? a, Entity? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Entity? a, Entity? b) => !(a == b);

        public bool Equals(Entity? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;
            if (Id == Guid.Empty || other.Id == Guid.Empty) return false;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => obj is Entity e && Equals(e);

        public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    }
}
