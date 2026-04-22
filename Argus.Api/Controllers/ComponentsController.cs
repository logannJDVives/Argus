using System;
using System.Text;
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
        private readonly IComponentService       _componentService;
        private readonly ICycloneDxExportService  _cycloneDxExportService;

        public ComponentsController(
            IComponentService      componentService,
            ICycloneDxExportService cycloneDxExportService)
        {
            _componentService       = componentService       ?? throw new ArgumentNullException(nameof(componentService));
            _cycloneDxExportService = cycloneDxExportService ?? throw new ArgumentNullException(nameof(cycloneDxExportService));
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

        /// <summary>
        /// Download de SBOM als CycloneDX JSON-bestand
        /// </summary>
        [HttpGet("export/cyclonedx")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportCycloneDx(Guid scanId)
        {
            var json  = await _cycloneDxExportService.GenerateCycloneDxJsonAsync(scanId);
            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"sbom-{scanId:N}.cdx.json");
        }
    }
}
