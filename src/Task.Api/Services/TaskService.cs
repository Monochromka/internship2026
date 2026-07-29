using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasks.Api.Data;
using Tasks.Api.Entities;
using Tasks.Api.Models;

namespace Tasks.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly IProjectsClient _projectsClient;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            AppDbContext context,
            IProjectsClient projectsClient,
            ILogger<TaskService> logger)
        {
            _context = context;
            _projectsClient = projectsClient;
            _logger = logger;
        }

        public async Task<TaskItem?> CreateTaskAsync(Guid projectId, CreateTaskDto request)
        {
            var project = await _projectsClient.GetProjectAsync(projectId);

            if (project == null)
            {
                _logger.LogWarning(
                    "Task creation skipped because project {ProjectId} was not found",
                    projectId);
                return null;
            }

            if (project.IsArchived)
            {
                _logger.LogWarning(
                    "Task creation failed because project {ProjectId} is archived",
                    projectId);
                throw new InvalidOperationException("Cannot add tasks to an archived project.");
            }

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = request.Title,
                Description = request.Description,
                Assignee = request.Assignee,
                DueDate = request.DueDate,
                Status = Entities.TaskStatus.ToDo,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Task {TaskId} created for project {ProjectId}",
                task.Id,
                projectId);

            return task;
        }



        public async Task<List<TaskItem>?> GetTasksByProjectIdAsync(Guid projectId, Entities.TaskStatus? status = null)
        {
            var project = await _projectsClient.GetProjectAsync(projectId);
            if (project == null)
            {
                _logger.LogWarning(
                    "Task list request failed because project {ProjectId} was not found",
                    projectId);
                return null;
            }

            var query = _context.Tasks.Where(t => t.ProjectId == projectId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tasks;
        }
        public async Task<TaskItem?> GetTaskByIdAsync(Guid projectId, Guid taskId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            return task;
        }

        public async Task<TaskItem?> UpdateTaskAsync(Guid projectId, Guid taskId, UpdateTaskDto request)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            if (task == null)
            {
                _logger.LogWarning(
                    "Task update failed because task {TaskId} in project {ProjectId} was not found",
                    taskId,
                    projectId);
                return null;
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Assignee = request.Assignee;
            task.DueDate = request.DueDate;

            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Task {TaskId} updated in project {ProjectId}",
                taskId,
                projectId);

            return task;
        }
        public async Task<(TaskItem? Task, bool IsConflict)> ChangeTaskStatusAsync(Guid projectId, Guid taskId, Entities.TaskStatus newStatus)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            if (task == null)
            {
                _logger.LogWarning(
                    "Task status change failed because task {TaskId} in project {ProjectId} was not found",
                    taskId,
                    projectId);
                return (null, false);
            }

            if (!task.CanTransitionTo(newStatus))
            {
                _logger.LogWarning(
                    "Task status change conflict for task {TaskId} in project {ProjectId}: current {CurrentStatus}, requested {RequestedStatus}",
                    taskId,
                    projectId,
                    task.Status,
                    newStatus);
                return (task, true);
            }

            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Task {TaskId} status changed to {Status} in project {ProjectId}",
                taskId,
                newStatus,
                projectId);

            return (task, false);
        }

        public async Task<bool> DeleteTaskAsync(Guid projectId, Guid taskId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            if (task == null)
            {
                _logger.LogWarning(
                    "Task deletion skipped because task {TaskId} in project {ProjectId} was not found",
                    taskId,
                    projectId);
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Task {TaskId} deleted from project {ProjectId}",
                taskId,
                projectId);

            return true;
        }

       
    }

}