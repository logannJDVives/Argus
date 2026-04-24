using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Argus.Dto.Scans;
using Argus.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Argus.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId}/scans")]
    public class ScansController : ControllerBase
    {
        private readonly IScanService _scanService;

        public ScansController(IScanService scanService)
        {
            _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
        }

        /// <summary>
        /// Start een nieuwe scan voor een project
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ScanRunDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScanRunDto>> StartScan(Guid projectId)
        {
            var result = await _scanService.StartScanAsync(projectId);
            return Accepted(result);
        }

        /// <summary>
        /// Haal alle scans op voor een project
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ScanRunDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ScanRunDto>>> GetScans(Guid projectId)
        {
            var results = await _scanService.GetScansByProjectAsync(projectId);
            return Ok(results);
        }

        /// <summary>
        /// Haal de laatste scan op voor een project
        /// </summary>
        [HttpGet("latest")]
        [ProducesResponseType(typeof(ScanRunDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScanRunDto>> GetLatestScan(Guid projectId)
        {
            var result = await _scanService.GetLatestScanAsync(projectId);
            return Ok(result);
        }

        /// <summary>
        /// Haal een specifieke scan op via ID
        /// </summary>
        [HttpGet("{scanId}")]
        [ProducesResponseType(typeof(ScanRunDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ScanRunDto>> GetScan(Guid projectId, Guid scanId)
        {
            var result = await _scanService.GetScanByIdAsync(scanId);
            return Ok(result);
        }
    }
}
