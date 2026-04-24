using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Argus.Dto.Projects;
using Argus.Interfaces;
using Argus.Services.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Argus.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IProjectUploadService _uploadService;

        public ProjectsController(IProjectService projectService, IProjectUploadService uploadService)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _uploadService  = uploadService  ?? throw new ArgumentNullException(nameof(uploadService));
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Create a new project
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _projectService.CreateProjectAsync(dto, UserId);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        /// <summary>
        /// Get all projects for the current user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProjectDto>>> GetAllProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync(UserId);
            return Ok(projects);
        }

        /// <summary>
        /// Get a single project by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
                return NotFound(new { message = $"Project with ID {id} not found." });

            return Ok(project);
        }

        /// <summary>
        /// Rename a project
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _projectService.UpdateProjectAsync(id, dto, UserId);

            if (updated == null)
                return NotFound(new { message = $"Project with ID {id} not found." });

            return Ok(updated);
        }

        /// <summary>
        /// Delete a project (cascades to scans and related data)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
                return NotFound(new { message = $"Project with ID {id} not found." });

            await _projectService.DeleteProjectAsync(id, UserId);
            return NoContent();
        }

        /// <summary>
        /// Upload a ZIP file and create a new project
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(200_000_000)] // 200 MB — Kestrel default is 28.6 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 200_000_000)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UploadProjectResponseDto>> UploadProject(IFormFile file, [FromForm] string projectName = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided." });

            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "File must be a ZIP archive." });

            try
            {
                var result = await _uploadService.UploadAndCreateProjectAsync(file, UserId);
                return CreatedAtAction(nameof(GetProject), new { id = result.ProjectId }, result);
            }
            catch (UploadValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
