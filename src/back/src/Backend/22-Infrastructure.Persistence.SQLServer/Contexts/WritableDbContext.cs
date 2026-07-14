using Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence.SQLServer.Contexts
{
    public class WritableDbContext : ApplicationDbContext
    {
        private readonly TimeProvider _timeProvider;

        public WritableDbContext() { }

        public WritableDbContext(
            DbContextOptions<ApplicationDbContext> options,
            TimeProvider timeProvider
        )
            : base(options)
        {
            _timeProvider = timeProvider;
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default
        )
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess,cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var now = _timeProvider.GetUtcNow();

            // Optimisation de performance : On matérialise une fois les entrées modifiées
            // pour éviter de scanner le ChangeTracker de manière répétée.
            var modifiedEntries = ChangeTracker
                .Entries()
                .Where(e =>
                    e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                )
                .ToDictionary(e => e.Entity, e => e.State);

            var timestampedEntries = ChangeTracker.Entries<ITimestampedEntity>().ToList();
            foreach (var entry in timestampedEntries)
            {
                bool hasDirectChanges = entry.State is EntityState.Added or EntityState.Modified;

                // On vérifie si les propriétés de navigation ont des changements
                bool hasNavigationChanges = HasChangedNavigationsRecursive(
                    entry,
                    modifiedEntries,
                    new HashSet<object>()
                );

                if (hasDirectChanges || hasNavigationChanges)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.CreatedAt = now;
                    }

                    entry.Entity.ModifiedAt = now;
                }
            }
        }

        private bool HasChangedNavigationsRecursive(
            EntityEntry entry,
            Dictionary<object, EntityState> modifiedEntries,
            HashSet<object> visited
        )
        {
            // Éviter les boucles infinies dans les relations circulaires
            if (!visited.Add(entry.Entity))
                return false;

            foreach (var navigation in entry.Navigations)
            {
                if (navigation.CurrentValue == null)
                    continue;

                // Cas 1 : La navigation est une collection
                if (navigation.CurrentValue is IEnumerable<object> collection)
                {
                    foreach (var item in collection)
                    {
                        if (IsModifiedOrDeepModified(item, modifiedEntries, visited))
                            return true;
                    }
                }
                // Cas 2 : La navigation est une référence simple
                else
                {
                    if (IsModifiedOrDeepModified(navigation.CurrentValue, modifiedEntries, visited))
                        return true;
                }
            }

            return false;
        }

        private bool IsModifiedOrDeepModified(
            object entity,
            Dictionary<object, EntityState> modifiedEntries,
            HashSet<object> visited
        )
        {
            // Si l'entité elle-même est marquée comme modifiée/ajoutée/supprimée
            if (modifiedEntries.ContainsKey(entity))
                return true;

            // Sinon, on plonge de manière récursive si c'est une entité suivie
            var entry = ChangeTracker.Entries().FirstOrDefault(e => e.Entity == entity);
            return entry != null && HasChangedNavigationsRecursive(entry, modifiedEntries, visited);
        }
    }
}
