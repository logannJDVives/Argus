using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Services.Abstractions;

namespace VivesRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderLinesController : ControllerBase
{
    private readonly IOrderLineService _orderLineService;

    public OrderLinesController(IOrderLineService orderLineService)
    {
        _orderLineService = orderLineService;
    }

    [HttpGet]
    public async Task<ActionResult<IList<OrderLineResult>>> Find()
    {
        var results = await _orderLineService.Find(null);
        var dtoResults = results.Select(r => new OrderLineResult
        {
            Id = r.Id,
            OrderId = r.OrderId,
            ArticleId = r.ArticleId,
            ProductName = r.ProductName,
            ProductDescription = r.ProductDescription,
            RentedAt = r.RentedAt,
            ExpiresAt = r.ExpiresAt,
            ReturnedAt = r.ReturnedAt
        }).ToList();
        return Ok(dtoResults);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderLineResult>> Get(Guid id)
    {
        var result = await _orderLineService.Get(id);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(new OrderLineResult
        {
            Id = result.Id,
            OrderId = result.OrderId,
            ArticleId = result.ArticleId,
            ProductName = result.ProductName,
            ProductDescription = result.ProductDescription,
            RentedAt = result.RentedAt,
            ExpiresAt = result.ExpiresAt,
            ReturnedAt = result.ReturnedAt
        });
    }

    [HttpPost("rent")]
    public async Task<ActionResult> Rent([FromBody] OrderLineRequest request)
    {
        var success = await _orderLineService.Rent(request.OrderId, request.ArticleId);
        if (!success)
        {
            return BadRequest();
        }
        return Ok();
    }

    [HttpPost("rent-multiple")]
    public async Task<ActionResult> RentMultiple([FromBody] RentMultipleRequest request)
    {
        var success = await _orderLineService.Rent(request.OrderId, request.ArticleIds);
        if (!success)
        {
            return BadRequest();
        }
        return Ok();
    }

    [HttpPatch("{id:guid}/return")]
    public async Task<ActionResult> Return(Guid id, [FromBody] DateTime returnedAt)
    {
        var success = await _orderLineService.Return(id, returnedAt);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }
}

public class RentMultipleRequest
{
    public Guid OrderId { get; set; }
    public IList<Guid> ArticleIds { get; set; } = new List<Guid>();
}
