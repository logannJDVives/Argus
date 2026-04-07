using System;
using System.Threading.Tasks;
using Argus.Dto.Secrets;
using Argus.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Argus.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class SecretsController : ControllerBase
    {
        private readonly ISecretService _secretService;

        public SecretsController(ISecretService secretService)
        {
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
        }

        /// <summary>
        /// Haal alle gevonden secrets op voor een scan (gepagineerd, optioneel gefilterd)
        /// </summary>
        [HttpGet("scans/{scanId}/secrets")]
        [ProducesResponseType(typeof(PaginatedSecretsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaginatedSecretsDto>> GetSecrets(
            Guid scanId,
            [FromQuery] string severity = null,
            [FromQuery] string filePath = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _secretService.GetSecretsByScanAsync(scanId, severity, filePath, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Markeer een secret als beoordeeld en/of false positive
        /// </summary>
        [HttpPatch("secrets/{id}/review")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReviewSecret(Guid id, [FromBody] ReviewSecretDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _secretService.ReviewSecretAsync(id, dto.IsReviewed, dto.IsFalsePositive);
            return NoContent();
        }
    }
}
