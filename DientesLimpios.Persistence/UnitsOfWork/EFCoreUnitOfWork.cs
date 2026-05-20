using DientesLimpios.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore.Storage;


namespace DientesLimpios.Persistence.UnitsOfWork
{
    public sealed class EFCoreUnitOfWork(DientesLimpiosDbContext context) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
                throw new InvalidOperationException("A transaction is already active.");

            _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                throw new InvalidOperationException("There is no active transaction to commit.");

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                return; // idempotent — nothing to roll back

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
                await _transaction.DisposeAsync();
        }
    }
}
