using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Projects.Api.Data;
using Projects.Api.Models;


namespace Projects.Api.Controllers
{
    [Route("api/v1/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public ProjectsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        // POST api/<ValuesController>
        [HttpPost]
        [ProducesResponseType(typeof(Entities.Project), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto request)
        {
            var newProject = new Entities.Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsArchived = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Projects.Add(newProject);
            await _dbContext.SaveChangesAsync();

            return Created($"/api/v1/projects/{newProject.Id}", newProject);

        }

      
    }
}
