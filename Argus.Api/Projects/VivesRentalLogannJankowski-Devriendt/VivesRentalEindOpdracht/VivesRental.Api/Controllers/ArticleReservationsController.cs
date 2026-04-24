using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Services.Abstractions;

namespace VivesRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticleReservationsController : ControllerBase
{
    private readonly IArticleReservationService _articleReservationService;

    public ArticleReservationsController(IArticleReservationService articleReservationService)
    {
        _articleReservationService = articleReservationService;
    }

    [HttpGet]
    public async Task<ActionResult<IList<ArticleReservationResult>>> Find()
    {
        var results = await _articleReservationService.Find(null);
        var dtoResults = results.Select(r => new ArticleReservationResult
        {
            Id = r.Id,
            ArticleId = r.ArticleId,
            ArticleStatus = r.ArticleStatus,
            ProductName = r.ProductName,
            CustomerId = r.CustomerId,
            CustomerFirstName = r.CustomerFirstName,
            CustomerLastName = r.CustomerLastName,
            CustomerEmail = r.CustomerEmail,
            FromDateTime = r.FromDateTime,
            UntilDateTime = r.UntilDateTime,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            ProcessedAt = r.ProcessedAt,
            RejectionReason = r.RejectionReason
        }).ToList();
        return Ok(dtoResults);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleReservationResult>> Get(Guid id)
    {
        var result = await _articleReservationService.Get(id);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(new ArticleReservationResult
        {
            Id = result.Id,
            ArticleId = result.ArticleId,
            ArticleStatus = result.ArticleStatus,
            ProductName = result.ProductName,
            CustomerId = result.CustomerId,
            CustomerFirstName = result.CustomerFirstName,
            CustomerLastName = result.CustomerLastName,
            CustomerEmail = result.CustomerEmail,
            FromDateTime = result.FromDateTime,
            UntilDateTime = result.UntilDateTime,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            ProcessedAt = result.ProcessedAt,
            RejectionReason = result.RejectionReason
        });
    }

    [HttpPost]
    public async Task<ActionResult<ArticleReservationResult>> Create([FromBody] ArticleReservationRequest request)
    {
        var serviceRequest = new VivesRental.Services.Model.Requests.ArticleReservationRequest
        {
            ArticleId = request.ArticleId,
            CustomerId = request.CustomerId,
            FromDateTime = request.FromDateTime,
            UntilDateTime = request.UntilDateTime
        };

        var result = await _articleReservationService.Create(serviceRequest);
        if (result is null)
        {
            return BadRequest(new { Message = "Kon reservering niet aanmaken. Artikel mogelijk niet beschikbaar." });
        }

        var dto = new ArticleReservationResult
        {
            Id = result.Id,
            ArticleId = result.ArticleId,
            ArticleStatus = result.ArticleStatus,
            ProductName = result.ProductName,
            CustomerId = result.CustomerId,
            CustomerFirstName = result.CustomerFirstName,
            CustomerLastName = result.CustomerLastName,
            CustomerEmail = result.CustomerEmail,
            FromDateTime = result.FromDateTime,
            UntilDateTime = result.UntilDateTime,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            ProcessedAt = result.ProcessedAt,
            RejectionReason = result.RejectionReason
        };
        return CreatedAtAction(nameof(Get), new { id = result.Id }, dto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _articleReservationService.Remove(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}