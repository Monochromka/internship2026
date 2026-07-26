using Tasks.Api.Models;

namespace Tasks.Api.Services
{
    public interface IProjectsClient
    {
        Task<ProjectDto?> GetProjectAsync(Guid projectId);
    }
}