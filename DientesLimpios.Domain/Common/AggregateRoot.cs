namespace DientesLimpios.Domain.Common
{
    public abstract class AggregateRoot : Entity
    {
        protected AggregateRoot(Guid id) : base(id) 
        { }
        protected AggregateRoot() { }  // EF Core

        // Audit fields
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Optimistic concurrency token. SQL Server maintains it; the value is only ever
        // read here. It is a plain byte[], so Domain still references nothing.
        public byte[] RowVersion { get; private set; } = [];

        // Domain events
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents
            => _domainEvents.AsReadOnly();

        protected void RaiseDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents() => _domainEvents.Clear();

    }
}
