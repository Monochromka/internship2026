using Tasks.Api.Entities;
using Tasks.Api.Models;

namespace Tasks.Api.Services
{
    public interface ITaskService
    {
        Task<TaskItem?> CreateTaskAsync(Guid projectId, CreateTaskDto request);

        Task<List<TaskItem>?> GetTasksByProjectIdAsync(Guid projectId);

        Task<TaskItem?> GetTaskByIdAsync(Guid projectId, Guid taskId);

        Task<TaskItem?> UpdateTaskAsync(Guid projectId, Guid taskId, UpdateTaskDto request);

        Task<(TaskItem? Task, bool IsConflict)> ChangeTaskStatusAsync(Guid projectId, Guid taskId, Entities.TaskStatus newStatus);

        Task<bool> DeleteTaskAsync(Guid projectId, Guid taskId);

    }
}