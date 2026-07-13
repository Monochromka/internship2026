using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Projects.Api.Data;
using Projects.Api.Entities;
using Projects.Api.Models;
using Projects.Api.Services;

namespace Projects.Api.Controllers
{
    [Route("api/v1/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }


        // POST api/<ValuesController>
        [HttpPost]
        [ProducesResponseType(typeof(Entities.Project), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto request)
        {
            var newProject = await _projectService.CreateProjectAsync(request);

            return Created($"/api/v1/projects/{newProject.Id}", newProject);

        }

        [HttpPatch("{id:guid}/archive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ArchiveProject(Guid id)
        {
            var project = await _projectService.ArchiveProjectAsync(id);

            if (project == null)
            {
                return NotFound();
            }
                
            return Ok(project);
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Entities.Project>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveProjects()
        {
            var projects = await _projectService.GetActiveProjectsAsync();

            return Ok(projects);
        }


        [HttpGet("{id:guid}")]        
        [ProducesResponseType(typeof(Entities.Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProjectById([FromRoute] Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedProject = await _projectService.UpdateProjectAsync(id, request);

                if (updatedProject == null)
                {
                    return NotFound();
                }

                return Ok(updatedProject);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
