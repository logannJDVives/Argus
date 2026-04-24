using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Services.Abstractions;

namespace VivesRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IList<OrderResult>>> Find()
    {
        var results = await _orderService.Find(null);
        var dtoResults = results.Select(r => new OrderResult
        {
            Id = r.Id,
            CustomerId = r.CustomerId,
            CustomerFirstName = r.CustomerFirstName,
            CustomerLastName = r.CustomerLastName,
            CustomerEmail = r.CustomerEmail,
            CustomerPhoneNumber = r.CustomerPhoneNumber,
            CreatedAt = r.CreatedAt,
            ReturnedAt = r.ReturnedAt,
            NumberOfOrderLines = r.NumberOfOrderLines
        }).ToList();
        return Ok(dtoResults);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResult>> Get(Guid id)
    {
        var result = await _orderService.Get(id);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(new OrderResult
        {
            Id = result.Id,
            CustomerId = result.CustomerId,
            CustomerFirstName = result.CustomerFirstName,
            CustomerLastName = result.CustomerLastName,
            CustomerEmail = result.CustomerEmail,
            CustomerPhoneNumber = result.CustomerPhoneNumber,
            CreatedAt = result.CreatedAt,
            ReturnedAt = result.ReturnedAt,
            NumberOfOrderLines = result.NumberOfOrderLines
        });
    }

    [HttpPost]
    public async Task<ActionResult<OrderResult>> Create([FromBody] OrderRequest request)
    {
        var result = await _orderService.Create(request.CustomerId);
        if (result is null)
        {
            return BadRequest();
        }

        var dto = new OrderResult
        {
            Id = result.Id,
            CustomerId = result.CustomerId,
            CustomerFirstName = result.CustomerFirstName,
            CustomerLastName = result.CustomerLastName,
            CustomerEmail = result.CustomerEmail,
            CustomerPhoneNumber = result.CustomerPhoneNumber,
            CreatedAt = result.CreatedAt,
            ReturnedAt = result.ReturnedAt,
            NumberOfOrderLines = result.NumberOfOrderLines
        };
        return CreatedAtAction(nameof(Get), new { id = result.Id }, dto);
    }

    [HttpPatch("{id:guid}/return")]
    public async Task<ActionResult> Return(Guid id, [FromBody] DateTime returnedAt)
    {
        var success = await _orderService.Return(id, returnedAt);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }
}
