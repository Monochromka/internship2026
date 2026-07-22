using Microsoft.AspNetCore.Mvc;
using Moq;
using Tasks.Api.Controllers;
using Tasks.Api.Models;
using Tasks.Api.Services;

namespace Tasks.Api.Tests
{
    public class TasksControllerTests
    {
        [Fact]
        public async Task ChangeTaskStatus_InvalidModelState_ReturnsBadRequest()
        {
            var service = new Mock<ITaskService>();
            var controller = new TasksController(service.Object);
            controller.ModelState.AddModelError(nameof(ChangeTaskStatusDto.Status), "The Status field is required.");

            var result = await controller.ChangeTaskStatus(Guid.NewGuid(), Guid.NewGuid(), new ChangeTaskStatusDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteTask_TaskExists_ReturnsNoContent()
        {
            var service = new Mock<ITaskService>();
            service.Setup(s => s.DeleteTaskAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);
            var controller = new TasksController(service.Object);

            var result = await controller.DeleteTask(Guid.NewGuid(), Guid.NewGuid());

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_TaskDoesNotExist_ReturnsNotFound()
        {
            var service = new Mock<ITaskService>();
            service.Setup(s => s.DeleteTaskAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);
            var controller = new TasksController(service.Object);

            var result = await controller.DeleteTask(Guid.NewGuid(), Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}