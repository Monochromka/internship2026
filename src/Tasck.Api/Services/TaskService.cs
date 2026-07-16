using Microsoft.EntityFrameworkCore;
using Tasks.Api.Data;
using Tasks.Api.Entities;
using Tasks.Api.Models;

namespace Tasks.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly IProjectsClient _projectsClient;

        public TaskService(AppDbContext context, IProjectsClient projectsClient)
        {
            _context = context;
            _projectsClient = projectsClient;
        }

        public async Task<TaskItem?> CreateTaskAsync(Guid projectId, CreateTaskDto request)
        {
            var project = await _projectsClient.GetProjectAsync(projectId);

            if (project == null) return null;

            if (project.IsArchived) throw new InvalidOperationException("Cannot add tasks to an archived project.");

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

            return task;
        }



        public async Task<List<TaskItem>?> GetTasksByProjectIdAsync(Guid projectId)
        {
            var project = await _projectsClient.GetProjectAsync(projectId);
            if (project == null)
            {
                return null; 
            }

            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
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
    }


}