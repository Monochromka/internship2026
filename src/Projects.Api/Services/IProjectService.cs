using Projects.Api.Entities;
using Projects.Api.Models;

namespace Projects.Api.Services
{
    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(CreateProjectDto request);

        Task<Project?> ArchiveProjectAsync(Guid id);

        Task<List<Project>> GetActiveProjectsAsync();

        Task<Project?> GetProjectByIdAsync(Guid id);
    }
}