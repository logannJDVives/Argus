using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Services.Abstractions;

namespace VivesRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<IList<CustomerResult>>> Find()
    {
        var results = await _customerService.Find(null);
        var dtoResults = results.Select(r => new CustomerResult
        {
            Id = r.Id,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Email = r.Email,
            PhoneNumber = r.PhoneNumber,
            NumberOfOrders = r.NumberOfOrders,
            NumberOfPendingOrders = r.NumberOfPendingOrders
        }).ToList();
        return Ok(dtoResults);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResult>> Get(Guid id)
    {
        var result = await _customerService.Get(id);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(new CustomerResult
        {
            Id = result.Id,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Email = result.Email,
            PhoneNumber = result.PhoneNumber,
            NumberOfOrders = result.NumberOfOrders,
            NumberOfPendingOrders = result.NumberOfPendingOrders
        });
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResult>> Create([FromBody] CustomerRequest request)
    {
        var serviceRequest = new VivesRental.Services.Model.Requests.CustomerRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _customerService.Create(serviceRequest);
        if (result is null)
        {
            return BadRequest();
        }

        var dto = new CustomerResult
        {
            Id = result.Id,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Email = result.Email,
            PhoneNumber = result.PhoneNumber,
            NumberOfOrders = result.NumberOfOrders,
            NumberOfPendingOrders = result.NumberOfPendingOrders
        };
        return CreatedAtAction(nameof(Get), new { id = result.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerResult>> Edit(Guid id, [FromBody] CustomerRequest request)
    {
        var serviceRequest = new VivesRental.Services.Model.Requests.CustomerRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _customerService.Edit(id, serviceRequest);
        if (result is null)
        {
            return NotFound();
        }
        return Ok(new CustomerResult
        {
            Id = result.Id,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Email = result.Email,
            PhoneNumber = result.PhoneNumber,
            NumberOfOrders = result.NumberOfOrders,
            NumberOfPendingOrders = result.NumberOfPendingOrders
        });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")] // Alleen Admin kan klanten verwijderen
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _customerService.Remove(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}
