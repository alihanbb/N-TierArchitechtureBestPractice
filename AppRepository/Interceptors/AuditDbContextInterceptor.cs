using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AppRepository.Interceptors
{
    public class AuditDbContextInterceptor : SaveChangesInterceptor
    {
        private static readonly Dictionary<EntityState,Action<DbContext,IAuditEntitiy>> _behaviors = new()
        {
            {EntityState.Added,AddBehavior},
            {EntityState.Modified,ModifedBehavior}
        };
        private static void AddBehavior(DbContext context, IAuditEntitiy auditEntitiy )
        {
            auditEntitiy.Created = DateTime.Now;
            context.Entry(auditEntitiy).Property(x => x.Created).IsModified = false;
        }
        private static void ModifedBehavior(DbContext context, IAuditEntitiy auditEntitiy)
        {
            context.Entry(auditEntitiy).Property(x => x.Updated).IsModified = false;
            auditEntitiy.Updated = DateTime.Now;
        }
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            foreach (var entityEntry in eventData.Context!.ChangeTracker.Entries().ToList())
            {
                if (entityEntry.Entity is not IAuditEntitiy auditEntitiy) continue;

                _behaviors[entityEntry.State](eventData.Context, auditEntitiy);


                //switch(entityEntry.State)
                //{
                //    case EntityState.Added:
                //        AddBehavior(eventData.Context, auditEntitiy);
                //        break;
                //    case EntityState.Modified:
                //        ModifedBehavior(eventData.Context, auditEntitiy);
                //        break;
                //}



            }
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
