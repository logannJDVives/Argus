using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;

namespace VivesRental.Sdk.Interfaces;

public interface IProductSdk
{
    Task<IList<ProductResult>> FindAsync();
    Task<ProductResult?> GetAsync(Guid id);
    Task<ProductResult?> CreateAsync(ProductRequest request);
    Task<ProductResult?> UpdateAsync(Guid id, ProductRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> GenerateArticlesAsync(Guid id, int amount);
}
