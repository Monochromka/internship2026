using Tasks.Api.Models;
using System.Net;

namespace Tasks.Api.Services
{
    public class ProjectsClient : IProjectsClient
    {
        private readonly HttpClient _httpClient;

        public ProjectsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProjectDto?> GetProjectAsync(Guid projectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/projects/{projectId}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ProjectDto>();
            }
            catch (HttpRequestException)
            {
                throw new ApplicationException("Projects.Api is unavailable.");
            }
        }
    }
}