using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;

namespace VivesRental.Sdk.Interfaces;

public interface IOrderSdk
{
    Task<IList<OrderResult>> FindAsync();
    Task<OrderResult?> GetAsync(Guid id);
    Task<OrderResult?> CreateAsync(OrderRequest request);
    Task<bool> ReturnAsync(Guid id, DateTime returnedAt);
}
