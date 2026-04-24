using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Services.Abstractions;

namespace VivesRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Alle endpoints vereisen authenticatie
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IList<ProductResult>>> Find()
    {
        var results = await _productService.Find(null);
        var dtoResults = results.Select(r => new ProductResult
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Manufacturer = r.Manufacturer,
            Publisher = r.Publisher,
            RentalExpiresAfterDays = r.RentalExpiresAfterDays,
            NumberOfArticles = r.NumberOfArticles,
            NumberOfAvailableArticles = r.NumberOfAvailableArticles
        }).ToList();
        return Ok(dtoResults);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResult>> Get(Guid id)
    {
        var result = await _productService.Get(id);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(new ProductResult
        {
            Id = result.Id,
            Name = result.Name,
            Description = result.Description,
            Manufacturer = result.Manufacturer,
            Publisher = result.Publisher,
            RentalExpiresAfterDays = result.RentalExpiresAfterDays,
            NumberOfArticles = result.NumberOfArticles,
            NumberOfAvailableArticles = result.NumberOfAvailableArticles
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Alleen Admin kan aanmaken
    public async Task<ActionResult<ProductResult>> Create([FromBody] ProductRequest request)
    {
        var serviceRequest = new VivesRental.Services.Model.Requests.ProductRequest
        {
            Name = request.Name,
            Description = request.Description,
            Manufacturer = request.Manufacturer,
            Publisher = request.Publisher,
            RentalExpiresAfterDays = request.RentalExpiresAfterDays
        };

        var result = await _productService.Create(serviceRequest);
        if (result is null)
        {
            return BadRequest();
        }

        var dto = new ProductResult
        {
            Id = result.Id,
            Name = result.Name,
            Description = result.Description,
            Manufacturer = result.Manufacturer,
            Publisher = result.Publisher,
            RentalExpiresAfterDays = result.RentalExpiresAfterDays,
            NumberOfArticles = result.NumberOfArticles,
            NumberOfAvailableArticles = result.NumberOfAvailableArticles
        };
        return CreatedAtAction(nameof(Get), new { id = result.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")] // Alleen Admin kan bewerken
    public async Task<ActionResult<ProductResult>> Edit(Guid id, [FromBody] ProductRequest request)
    {
        var serviceRequest = new VivesRental.Services.Model.Requests.ProductRequest
        {
            Name = request.Name,
            Description = request.Description,
            Manufacturer = request.Manufacturer,
            Publisher = request.Publisher,
            RentalExpiresAfterDays = request.RentalExpiresAfterDays
        };

        var result = await _productService.Edit(id, serviceRequest);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(new ProductResult
        {
            Id = result.Id,
            Name = result.Name,
            Description = result.Description,
            Manufacturer = result.Manufacturer,
            Publisher = result.Publisher,
            RentalExpiresAfterDays = result.RentalExpiresAfterDays,
            NumberOfArticles = result.NumberOfArticles,
            NumberOfAvailableArticles = result.NumberOfAvailableArticles
        });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")] // Alleen Admin kan verwijderen
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _productService.Remove(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id:guid}/generate-articles")]
    [Authorize(Roles = "Admin")] // Alleen Admin kan artikelen genereren
    public async Task<ActionResult> GenerateArticles(Guid id, [FromQuery] int amount = 1)
    {
        var success = await _productService.GenerateArticles(id, amount);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }
}
