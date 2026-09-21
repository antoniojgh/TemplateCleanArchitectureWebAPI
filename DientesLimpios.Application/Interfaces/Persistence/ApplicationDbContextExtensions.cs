using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Application.Interfaces.Persistence
{
    public static class ApplicationDbContextExtensions
    {
        // A lost update is an expected outcome, not a fault: someone else changed the row
        // between our read and our write. It travels as a Result like every other outcome,
        // and Concurrency.Conflict maps to 409 by the error-code suffix convention.
        public static async Task<Result> SaveChangesAsResult(this IApplicationDbContext db,
                                                             CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(db);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure(DomainErrors.Concurrency.Conflict);
            }
        }
    }
}