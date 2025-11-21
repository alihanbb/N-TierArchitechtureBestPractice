using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AppRepository.Interceptors
{
    public class AuditDbContextInterceptor : SaveChangesInterceptor
    {
        private static readonly Dictionary<EntityState, Action<DbContext, IAuditEntitiy>> _behaviors = new()
        {
            { EntityState.Added, AddBehavior },
            { EntityState.Modified, ModifiedBehavior }
        };

        private static void AddBehavior(DbContext context, IAuditEntitiy auditEntity)
        {
            auditEntity.Created = DateTime.UtcNow;
        }

        private static void ModifiedBehavior(DbContext context, IAuditEntitiy auditEntity)
        {
            auditEntity.Updated = DateTime.UtcNow;
            // Created alanının güncellenmesini engelle
            context.Entry(auditEntity).Property(x => x.Created).IsModified = false;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            SetAuditProperties(eventData.Context!);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            SetAuditProperties(eventData.Context!);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static void SetAuditProperties(DbContext context)
        {
            foreach (var entityEntry in context.ChangeTracker.Entries().ToList())
            {
                if (entityEntry.Entity is not IAuditEntitiy auditEntity) continue;

                // Sadece Added ve Modified state'lerinde işlem yap
                if (_behaviors.TryGetValue(entityEntry.State, out var behavior))
                {
                    behavior(context, auditEntity);
                }
            }
        }
    }
}
