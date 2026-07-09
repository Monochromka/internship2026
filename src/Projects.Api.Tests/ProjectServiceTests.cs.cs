using Microsoft.EntityFrameworkCore;
using Projects.Api.Data;
using Projects.Api.Entities;
using Projects.Api.Services;
using Xunit;

namespace Projects.Api.Tests
{
    public class ProjectServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetActiveProjectsAsync_ShouldReturnOnlyNonArchived_OrderedByDateDesc()
        {
            var context = GetInMemoryDbContext();
            var service = new ProjectService(context);

            context.Projects.AddRange(
                new Project { Id = Guid.NewGuid(), Name = "Old Active", IsArchived = false, CreatedAt = DateTimeOffset.UtcNow.AddDays(-2) },
                new Project { Id = Guid.NewGuid(), Name = "New Active", IsArchived = false, CreatedAt = DateTimeOffset.UtcNow },
                new Project { Id = Guid.NewGuid(), Name = "Archived", IsArchived = true, CreatedAt = DateTimeOffset.UtcNow }
            );
            await context.SaveChangesAsync();

            var result = await service.GetActiveProjectsAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("New Active", result[0].Name);
            Assert.Equal("Old Active", result[1].Name);
        }

        [Fact]
        public async Task GetActiveProjectsAsync_WhenNoProjects_ShouldReturnEmptyList()
        {
            
            var context = GetInMemoryDbContext(); 
            var service = new ProjectService(context);

            var result = await service.GetActiveProjectsAsync();

            Assert.Empty(result);
        }
    }
}