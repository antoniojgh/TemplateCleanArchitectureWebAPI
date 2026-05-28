using DientesLimpios.Application.Interfaces.Identity;
using DientesLimpios.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace DientesLimpios.Persistence.Interceptors
{
    public sealed class AuditableEntitiesInterceptor(IUserService userService) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, 
                                                            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var now = DateTime.UtcNow;
            var userId = userService.GetUserId();

            foreach (EntityEntry<AggregateRoot> entry in
                     eventData.Context.ChangeTracker.Entries<AggregateRoot>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        entry.Entity.CreatedBy = userId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = now;
                        entry.Entity.LastModifiedBy = userId;
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
