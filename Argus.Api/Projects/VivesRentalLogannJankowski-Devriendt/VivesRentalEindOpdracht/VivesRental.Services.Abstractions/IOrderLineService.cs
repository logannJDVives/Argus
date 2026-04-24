using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services.Abstractions;

public interface IOrderLineService
{
    Task<OrderLineResult?> Get(Guid id);
    Task<bool> Rent(Guid orderId, Guid articleId);
    Task<bool> Rent(Guid orderId, IList<Guid> articleIds);
    Task<bool> Return(Guid orderLineId, DateTime returnedAt);
    Task<List<OrderLineResult>> Find(OrderLineFilter? filter);

}