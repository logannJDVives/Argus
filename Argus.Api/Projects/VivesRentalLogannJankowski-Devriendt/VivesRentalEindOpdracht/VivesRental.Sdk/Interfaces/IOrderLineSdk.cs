using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;

namespace VivesRental.Sdk.Interfaces;

public interface IOrderLineSdk
{
    Task<IList<OrderLineResult>> FindAsync();
    Task<OrderLineResult?> GetAsync(Guid id);
    Task<bool> RentAsync(OrderLineRequest request);
    Task<bool> RentMultipleAsync(Guid orderId, IList<Guid> articleIds);
    Task<bool> ReturnAsync(Guid id, DateTime returnedAt);
}
