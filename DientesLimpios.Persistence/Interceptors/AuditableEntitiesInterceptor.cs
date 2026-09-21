using DientesLimpios.Application.Interfaces.Identity;
using DientesLimpios.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace DientesLimpios.Persistence.Interceptors
{
    // Stamps the IAuditable fields from the ambient user and clock. The aggregates keep their
    // setters private, so the values are written through the change tracker, which reaches
    // them regardless of accessibility.
    public sealed class AuditableEntitiesInterceptor(IUserService userService, TimeProvider timeProvider)
        : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
                ApplyAudit(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // The previous interceptor overrode only the async method, so a synchronous SaveChanges
        // left CreatedDate at default(DateTime) — which UtcDateTimeConverter then rejects,
        // turning a missing audit stamp into a failed save.
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
                ApplyAudit(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        private void ApplyAudit(DbContext context)
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var userId = userService.GetUserId();

            foreach (EntityEntry<IAuditable> entry in context.ChangeTracker.Entries<IAuditable>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property<DateTime>(nameof(IAuditable.CreatedDate)).CurrentValue = now;
                        entry.Property<string?>(nameof(IAuditable.CreatedBy)).CurrentValue = userId;
                        break;
                    case EntityState.Modified:
                        entry.Property<DateTime?>(nameof(IAuditable.LastModifiedDate)).CurrentValue = now;
                        entry.Property<string?>(nameof(IAuditable.LastModifiedBy)).CurrentValue = userId;
                        break;
                }
            }
        }
    }
}
