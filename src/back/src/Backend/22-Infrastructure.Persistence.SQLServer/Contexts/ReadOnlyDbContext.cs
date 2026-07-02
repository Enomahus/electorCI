using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.SQLServer.Contexts
{
    public class ReadOnlyDbContext : ApplicationDbContext
    {
        public ReadOnlyDbContext() { }

        public ReadOnlyDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
    }
}
