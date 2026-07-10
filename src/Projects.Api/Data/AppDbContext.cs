using Microsoft.EntityFrameworkCore;
using Projects.Api.Entities; 

namespace Projects.Api.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Entities.Project> Projects { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Project>()
                .ToContainer("projects")
                .HasPartitionKey(p => p.Id);
        }
    }
}