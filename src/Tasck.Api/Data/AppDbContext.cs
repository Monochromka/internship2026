using Microsoft.EntityFrameworkCore;
using Tasks.Api.Entities;

namespace Tasks.Api.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskItem> Tasks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>()
                .ToContainer("tasks") 
                .HasNoDiscriminator() 
                .HasPartitionKey(t => t.ProjectId); 
        }
    }
}