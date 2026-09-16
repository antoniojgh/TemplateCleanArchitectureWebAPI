namespace DientesLimpios.Persistence.Outbox
{
    // A domain event stored in the same transaction as the aggregate that raised it.
    public sealed class OutboxMessage
    {
        public const int ErrorMaxLength = 2000;

        public Guid Id { get; private set; }
        public string Type { get; private set; } = null!;
        public string Payload { get; private set; } = null!;
        public DateTime OccurredOnUtc { get; private set; }
        public DateTime? ProcessedOnUtc { get; private set; }
        public int AttemptCount { get; private set; }
        public string? Error { get; private set; }

        private OutboxMessage() { }   // EF Core

        public OutboxMessage(Guid id, string type, string payload, DateTime occurredOnUtc)
        {
            Id = id;
            Type = type;
            Payload = payload;
            OccurredOnUtc = occurredOnUtc;
        }

        public void MarkProcessed(DateTime nowUtc)
        {
            ProcessedOnUtc = nowUtc;
            Error = null;
        }

        public void MarkFailed(string error)
        {
            AttemptCount++;
            Error = error.Length > ErrorMaxLength ? error[..ErrorMaxLength] : error;
        }
    }
}