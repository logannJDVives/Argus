using System;
using System.Threading.Tasks;
using Argus.Dto.Components;
using Argus.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Argus.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/scans/{scanId}/components")]
    public class ComponentsController : ControllerBase
    {
        private readonly IComponentService _componentService;

        public ComponentsController(IComponentService componentService)
        {
            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));
        }

        /// <summary>
        /// Haal alle software componenten (SBOM) op voor een scan (gepagineerd, optioneel gefilterd)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedComponentsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaginatedComponentsDto>> GetComponents(
            Guid   scanId,
            [FromQuery] string search   = null,
            [FromQuery] int    page     = 1,
            [FromQuery] int    pageSize = 25)
        {
            var result = await _componentService.GetComponentsByScanAsync(scanId, search, page, pageSize);
            return Ok(result);
        }
    }
}
