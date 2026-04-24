using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;

namespace VivesRental.Sdk.Interfaces;

public interface ICustomerSdk
{
    Task<IList<CustomerResult>> FindAsync();
    Task<CustomerResult?> GetAsync(Guid id);
    Task<CustomerResult?> CreateAsync(CustomerRequest request);
    Task<CustomerResult?> UpdateAsync(Guid id, CustomerRequest request);
    Task<bool> DeleteAsync(Guid id);
}
