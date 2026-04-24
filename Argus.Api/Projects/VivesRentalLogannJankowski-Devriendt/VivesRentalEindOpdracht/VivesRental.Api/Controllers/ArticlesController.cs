using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Enums;
using VivesRental.Services.Abstractions;
using VivesRental.Services.Model.Filters;

namespace VivesRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    [HttpGet]
    public async Task<ActionResult<IList<ArticleResult>>> Find([FromQuery] Guid? productId)
    {
        var filter = productId.HasValue ? new ArticleFilter { ProductId = productId.Value } : null;
        var results = await _articleService.Find(filter);
        var dtoResults = results.Select(r => new ArticleResult
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.ProductName,
            Status = r.Status
        }).ToList();
        return Ok(dtoResults);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleResult>> Get(Guid id)
    {
        var result = await _articleService.Get(id);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(new ArticleResult
        {
            Id = result.Id,
            ProductId = result.ProductId,
            ProductName = result.ProductName,
            Status = result.Status
        });
    }

    [HttpGet("available/{productId:guid}")]
    public async Task<ActionResult<ArticleResult?>> GetFirstAvailable(Guid productId)
    {
        var filter = new ArticleFilter 
        { 
            ProductId = productId,
            AvailableFromDateTime = DateTime.Now
        };
        var results = await _articleService.Find(filter);
        var first = results.FirstOrDefault();
        
        if (first is null)
        {
            return NotFound();
        }

        return Ok(new ArticleResult
        {
            Id = first.Id,
            ProductId = first.ProductId,
            ProductName = first.ProductName,
            Status = first.Status
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ArticleResult>> Create([FromBody] ArticleRequest request)
    {
        var serviceRequest = new VivesRental.Services.Model.Requests.ArticleRequest
        {
            ProductId = request.ProductId
        };

        var result = await _articleService.Create(serviceRequest);
        if (result is null)
        {
            return BadRequest();
        }

        var dto = new ArticleResult
        {
            Id = result.Id,
            ProductId = result.ProductId,
            ProductName = result.ProductName,
            Status = result.Status
        };
        return CreatedAtAction(nameof(Get), new { id = result.Id }, dto);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] ArticleStatus status)
    {
        var success = await _articleService.UpdateStatus(id, status);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _articleService.Remove(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}
