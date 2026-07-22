using Microsoft.AspNetCore.Mvc;
using Tasks.Api.Models;
using Tasks.Api.Services;

namespace Tasks.Api.Controllers
{
    [ApiController]
    [Route("api/v1/projects/{projectId}/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Entities.TaskItem), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var task = await _taskService.CreateTaskAsync(projectId, request);

                if (task == null)
                {
                    return NotFound(new { message = $"Project with ID {projectId} not found." });
                }

                return StatusCode(201, task);
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ApplicationException ex) 
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Entities.TaskItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetTasks(Guid projectId)
        {
            try
            {
                var tasks = await _taskService.GetTasksByProjectIdAsync(projectId);

                if (tasks == null)
                {
                    return NotFound(new { message = $"Project with ID {projectId} not found." });
                }

                return Ok(tasks);
            }
            catch (ApplicationException ex) 
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }

        [HttpGet("{taskId}")]
        [ProducesResponseType(typeof(Entities.TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTaskById(Guid projectId, Guid taskId)
        {
            var task = await _taskService.GetTaskByIdAsync(projectId, taskId);

            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {taskId} not found in project {projectId}." });
            }

            return Ok(task);
        }


        [HttpPut("{taskId}")]
        [ProducesResponseType(typeof(Entities.TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId, [FromBody] UpdateTaskDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedTask = await _taskService.UpdateTaskAsync(projectId, taskId, request);

            if (updatedTask == null)
            {
                return NotFound(new { message = $"Task with ID {taskId} not found in project {projectId}." });
            }

            return Ok(updatedTask);
        }

        [HttpPatch("{taskId}/status")]
        [ProducesResponseType(typeof(Entities.TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ChangeTaskStatus(Guid projectId, Guid taskId, [FromBody] ChangeTaskStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (task, isConflict) = await _taskService.ChangeTaskStatusAsync(projectId, taskId, request.Status);

            if (task == null)
            {
                return NotFound(); 
            }

            if (isConflict)
            {
                return Conflict(new { message = $"Cannot transition task from {task.Status} to {request.Status}." });
            }

            return Ok(task); 
        }

        [HttpDelete("{taskId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
        {
            var isDeleted = await _taskService.DeleteTaskAsync(projectId, taskId);

            if (!isDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}