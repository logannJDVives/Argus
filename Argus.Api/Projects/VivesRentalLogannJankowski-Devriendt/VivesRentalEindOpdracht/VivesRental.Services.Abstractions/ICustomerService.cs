using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Requests;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services.Abstractions;

public interface ICustomerService
{
    Task<CustomerResult?> Get(Guid id);
    Task<List<CustomerResult>> Find(CustomerFilter? filter);
    Task<CustomerResult?> Create(CustomerRequest entity);
    Task<CustomerResult?> Edit(Guid id, CustomerRequest entity);
    Task<bool> Remove(Guid id);
}