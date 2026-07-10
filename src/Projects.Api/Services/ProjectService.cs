using Microsoft.EntityFrameworkCore;
using Projects.Api.Data;
using Projects.Api.Entities;
using Projects.Api.Models;

namespace Projects.Api.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _dbContext;

        public ProjectService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Project> CreateProjectAsync(CreateProjectDto request)
        {
            var newProject = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsArchived = false, 
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Projects.Add(newProject);
            await _dbContext.SaveChangesAsync();

            return newProject;
        }

        public async Task<Project?> ArchiveProjectAsync(Guid id)
        {
            var project = await _dbContext.Projects.FindAsync(id);

            if (project == null)
            {
                return null;
            }

            if (!project.IsArchived)
            {
                project.IsArchived = true;
                await _dbContext.SaveChangesAsync();
            }

            return project;
        }

        public async Task<List<Project>> GetActiveProjectsAsync()
        {
            return await _dbContext.Projects
                .Where(p => !p.IsArchived)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(Guid id)
        {
            return await _dbContext.Projects.FindAsync(id);
        }
    }
}