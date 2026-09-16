using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Outbox
{
    public static class OutboxMessageQueries
    {
        // Returns the next unprocessed messages and locks them so that no other API instance
        // can take them.
        //
        // UPDLOCK  holds an update lock on each returned row until the transaction ends. It
        //          does not block plain readers, unlike XLOCK.
        // READPAST makes other instances skip locked rows instead of waiting, so each
        //          instance takes a different batch.
        // ROWLOCK  keeps the locks at row level, so skipping one row never hides its neighbours.
        //
        // The locks last only as long as the transaction, so calling this without one would
        // protect nothing. It throws instead.
        public static Task<List<OutboxMessage>> LockNextOutboxBatch(this DientesLimpiosDbContext context,
            int batchSize, int maxAttempts, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Database.CurrentTransaction is null)
                throw new InvalidOperationException(
                    "LockNextOutboxBatch must run inside a transaction; its row locks end with it.");

            // Returned as a list, not IQueryable: composing more LINQ on top of FromSql would
            // wrap this statement in a subquery.
            return context.OutboxMessages
                .FromSql($"""
                    SELECT TOP ({batchSize}) *
                    FROM dbo.OutboxMessages WITH (UPDLOCK, READPAST, ROWLOCK)
                    WHERE ProcessedOnUtc IS NULL AND AttemptCount < {maxAttempts}
                    ORDER BY OccurredOnUtc
                    """)
                .ToListAsync(cancellationToken);
        }
    }
}