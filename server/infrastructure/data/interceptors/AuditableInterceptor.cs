using domain.interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace infrastructure.data.interceptors;

public class AuditableInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var entries = eventData.Context?.ChangeTracker
            .Entries<IAuditableEntity>() ?? [];

        foreach (var entry in entries)
        {
            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
