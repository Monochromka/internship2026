using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Projects.Api.Data;
using Projects.Api.Entities;
using Projects.Api.Models;
using Projects.Api.Services;
using System.ComponentModel.DataAnnotations;
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


        [Fact]
        public async Task GetProjectByIdAsync_ExistingId_ShouldReturnProject()
        {
            var context = GetInMemoryDbContext();
            var service = new ProjectService(context);
            var projectId = Guid.NewGuid();

            context.Projects.Add(new Project { Id = projectId, Name = "Target Project" });
            await context.SaveChangesAsync();

            var result = await service.GetProjectByIdAsync(projectId);

            Assert.NotNull(result);
            Assert.Equal("Target Project", result.Name);
        }

        [Fact]
        public async Task GetProjectByIdAsync_NonExistingId_ShouldReturnNull()
        {
            var context = GetInMemoryDbContext();
            var service = new ProjectService(context);

            var result = await service.GetProjectByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }



        [Fact]
        public void Validate_EmptyName_ReturnsValidationError()
        {
            // Arrange
            var dto = new UpdateProjectDto { Name = "", Description = "Valid desc" };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Validate_NameExceeds100Characters_ReturnsValidationError()
        {
            // Arrange
            var dto = new UpdateProjectDto
            {
                Name = new string('a', 101),
                Description = "Valid desc"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        // Допоміжний метод для імітації перевірки атрибутів DataAnnotations
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }


        [Fact]
        public async Task UpdateProjectAsync_WithExistingProject_UpdatesAndReturnsProject()
        {
            // Arrange: створюємо базу і сервіс так само, як у твоїх інших тестах
            var context = GetInMemoryDbContext();
            var service = new ProjectService(context);

            var projectId = Guid.NewGuid();
            var existingProject = new Project
            {
                Id = projectId,
                Name = "Old Name",
                Description = "Old Desc"
            };
            context.Projects.Add(existingProject);
            await context.SaveChangesAsync();

            var updateRequest = new UpdateProjectDto { Name = "New Name", Description = "New Desc" };

            // Act
            var result = await service.UpdateProjectAsync(projectId, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("New Name");
            result.Description.Should().Be("New Desc");
        }

        [Fact]
        public async Task UpdateProjectAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange: створюємо базу і сервіс
            var context = GetInMemoryDbContext();
            var service = new ProjectService(context);

            var nonExistentId = Guid.NewGuid();
            var updateRequest = new UpdateProjectDto { Name = "Valid Name", Description = "Valid Desc" };

            // Act
            var result = await service.UpdateProjectAsync(nonExistentId, updateRequest);

            // Assert
            result.Should().BeNull();
        }
    }
}