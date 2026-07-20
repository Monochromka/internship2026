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

        [Fact]
        public async Task GetTasksByProjectIdAsync_ProjectExists_ReturnsSortedTasks()
        {
            // Arrange (Підготовка)
            using var context = new AppDbContext(_dbContextOptions);
            var mockProjectsClient = new Mock<IProjectsClient>();
            var projectId = Guid.NewGuid();

            // Мокаємо успішну відповідь, що проєкт існує
            mockProjectsClient.Setup(client => client.GetProjectAsync(projectId))
                .ReturnsAsync(new ProjectDto { Id = projectId, IsArchived = false });

            // Створюємо таски з різним часом створення
            context.Tasks.AddRange(
                new TaskItem { Id = Guid.NewGuid(), ProjectId = projectId, Title = "Стара таска", CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new TaskItem { Id = Guid.NewGuid(), ProjectId = projectId, Title = "Нова таска", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = new TaskService(context, mockProjectsClient.Object);

            // Act (Дія)
            var result = await service.GetTasksByProjectIdAsync(projectId);

            // Assert (Перевірка)
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            // Перевіряємо, що першою йде саме НОВА таска (сортування по спаданню)
            Assert.Equal("Нова таска", result.First().Title);
        }

        [Fact]
        public async Task GetTasksByProjectIdAsync_ProjectDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            var mockProjectsClient = new Mock<IProjectsClient>();
            var projectId = Guid.NewGuid();

            // Мокаємо ситуацію, коли проєкт не знайдено (повертається null)
            mockProjectsClient.Setup(client => client.GetProjectAsync(projectId))
                .ReturnsAsync((ProjectDto?)null);

            var service = new TaskService(context, mockProjectsClient.Object);

            // Act
            var result = await service.GetTasksByProjectIdAsync(projectId);

            // Assert
            Assert.Null(result); // Має повернутися null, щоб контролер видав 404
        }

        [Fact]
        public async Task GetTaskByIdAsync_TaskExists_ReturnsTask()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            var mockProjectsClient = new Mock<IProjectsClient>();
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            context.Tasks.Add(new TaskItem
            {
                Id = taskId,
                ProjectId = projectId,
                Title = "Знайди мене"
            });
            await context.SaveChangesAsync();

            var service = new TaskService(context, mockProjectsClient.Object);

            // Act
            var result = await service.GetTaskByIdAsync(projectId, taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
            Assert.Equal("Знайди мене", result.Title);
        }

        [Fact]
        public async Task GetTaskByIdAsync_TaskDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            var mockProjectsClient = new Mock<IProjectsClient>();

            var service = new TaskService(context, mockProjectsClient.Object);

            // Act (Шукаємо випадкові ID, яких немає в базі)
            var result = await service.GetTaskByIdAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_TaskExists_UpdatesAllowedFieldsAndTimestamp()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            var mockProjectsClient = new Mock<IProjectsClient>();
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var originalTime = DateTime.UtcNow.AddDays(-1);
            var originalStatus = Entities.TaskStatus.ToDo;

            // Створюємо таску зі старими даними
            var existingTask = new TaskItem
            {
                Id = taskId,
                ProjectId = projectId,
                Title = "Old Title",
                Description = "Old Description",
                Status = originalStatus,
                CreatedAt = originalTime,
                UpdatedAt = originalTime
            };
            context.Tasks.Add(existingTask);
            await context.SaveChangesAsync();

            var service = new TaskService(context, mockProjectsClient.Object);

            var updateRequest = new UpdateTaskDto
            {
                Title = "New Title",
                Description = "New Description",
                Assignee = "New Assignee",
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            // Act
            var result = await service.UpdateTaskAsync(projectId, taskId, updateRequest);

            // Assert
            Assert.NotNull(result);
            // Перевіряємо, що дозволені поля змінилися
            Assert.Equal("New Title", result.Title);
            Assert.Equal("New Description", result.Description);
            Assert.Equal("New Assignee", result.Assignee);
            Assert.Equal(updateRequest.DueDate, result.DueDate);

            // Перевіряємо, що UpdatedAt оновився, а CreatedAt та Status залишилися старими
            Assert.True(result.UpdatedAt > originalTime);
            Assert.Equal(originalTime, result.CreatedAt);
            Assert.Equal(originalStatus, result.Status);
        }

        [Fact]
        public async Task UpdateTaskAsync_TaskDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            var mockProjectsClient = new Mock<IProjectsClient>();
            var service = new TaskService(context, mockProjectsClient.Object);

            var updateRequest = new UpdateTaskDto { Title = "Doesn't matter" };

            // Act (передаємо неіснуючі ID)
            var result = await service.UpdateTaskAsync(Guid.NewGuid(), Guid.NewGuid(), updateRequest);

            // Assert
            Assert.Null(result);
        }
    }


}