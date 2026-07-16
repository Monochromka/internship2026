using Microsoft.EntityFrameworkCore;
using Moq;
using Tasks.Api.Data;
using Tasks.Api.Entities;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;

namespace Tasks.Api.Tests
{
    public class TaskServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public TaskServiceTests()
        {
            // Використовуємо базу даних в пам'яті для швидкого тестування
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CreateTaskAsync_WithValidProject_ReturnsTask()
        {
            // Arrange (Підготовка)
            using var context = new AppDbContext(_dbContextOptions);
            var mockProjectsClient = new Mock<IProjectsClient>();
            var projectId = Guid.NewGuid();

            // Мокаємо успішну відповідь від Projects.Api (проєкт існує і не в архіві)
            mockProjectsClient.Setup(client => client.GetProjectAsync(projectId))
                .ReturnsAsync(new ProjectDto { Id = projectId, IsArchived = false });

            var service = new TaskService(context, mockProjectsClient.Object);
            var request = new CreateTaskDto { Title = "Implement create project endpoint" };

            // Act (Дія)
            var result = await service.CreateTaskAsync(projectId, request);

            // Assert (Перевірка)
            Assert.NotNull(result);
            Assert.Equal("Implement create project endpoint", result.Title);
            Assert.Equal(projectId, result.ProjectId);
            Assert.Equal(Entities.TaskStatus.ToDo, result.Status);

            // Перевіряємо, чи таска дійсно збереглася в базу
            Assert.Single(context.Tasks);
        }

        [Fact]
        public async Task CreateTaskAsync_WithArchivedProject_ThrowsException()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            var mockProjectsClient = new Mock<IProjectsClient>();
            var projectId = Guid.NewGuid();

            // Мокаємо заархівований проєкт
            mockProjectsClient.Setup(client => client.GetProjectAsync(projectId))
                .ReturnsAsync(new ProjectDto { Id = projectId, IsArchived = true });

            var service = new TaskService(context, mockProjectsClient.Object);
            var request = new CreateTaskDto { Title = "Test Task" };

            // Act & Assert (Має викинути InvalidOperationException)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateTaskAsync(projectId, request));
        }
    }
}