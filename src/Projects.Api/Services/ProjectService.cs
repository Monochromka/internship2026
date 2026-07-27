using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Projects.Api.Data;
using Projects.Api.Entities;
using Projects.Api.Models;

namespace Projects.Api.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(AppDbContext dbContext, ILogger<ProjectService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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

            _logger.LogInformation(
                "Project {ProjectId} created",
                newProject.Id);

            return newProject;
        }

        public async Task<Project?> ArchiveProjectAsync(Guid id)
        {
            var project = await _dbContext.Projects.FindAsync(id);

            if (project == null)
            {
                _logger.LogWarning(
                    "Archive skipped because project {ProjectId} was not found",
                    id);
                return null;
            }

            if (!project.IsArchived)
            {
                project.IsArchived = true;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Project {ProjectId} archived",
                    id);
            }
            else
            {
                _logger.LogWarning(
                    "Archive skipped because project {ProjectId} is already archived",
                    id);
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

        public async Task<Project?> UpdateProjectAsync(Guid id, UpdateProjectDto request)
        {
            var project = await _dbContext.Projects.FindAsync(id);

            if (project == null)
            {
                _logger.LogWarning(
                    "Update failed because project {ProjectId} was not found",
                    id);
                return null;
            }

          
            if (project.IsArchived)
            {
                _logger.LogWarning(
                    "Update failed because project {ProjectId} is archived",
                    id);
                throw new InvalidOperationException("Cannot update an archived project.");
            }

            project.Name = request.Name;
            project.Description = request.Description;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Project {ProjectId} updated",
                id);

            return project;
        }
    }
}