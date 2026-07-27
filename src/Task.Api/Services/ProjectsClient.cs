using Tasks.Api.Models;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Tasks.Api.Services
{
    public class ProjectsClient : IProjectsClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProjectsClient> _logger;

        public ProjectsClient(HttpClient httpClient, ILogger<ProjectsClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ProjectDto?> GetProjectAsync(Guid projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/projects/{projectId}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(
                        "Projects.Api returned not found for project {ProjectId}",
                        projectId);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ProjectDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve project {ProjectId} from Projects.Api",
                    projectId);
                throw new ApplicationException("Projects.Api is unavailable.");
            }
        }
    }
}