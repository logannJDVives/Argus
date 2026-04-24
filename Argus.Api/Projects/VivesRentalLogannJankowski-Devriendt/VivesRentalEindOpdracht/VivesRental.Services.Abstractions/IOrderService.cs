using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services.Abstractions;

public interface IOrderService
{
    Task<OrderResult?> Get(Guid id);

    Task<List<OrderResult>> Find(OrderFilter? filter);

    Task<OrderResult?> Create(Guid customerId);
    Task<bool> Return(Guid id, DateTime returnedAt);
}