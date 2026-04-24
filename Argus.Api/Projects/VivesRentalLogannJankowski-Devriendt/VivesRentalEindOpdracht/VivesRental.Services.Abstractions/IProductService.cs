using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Requests;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services.Abstractions;

public interface IProductService
{
    Task<ProductResult?> Get(Guid id);
    Task<List<ProductResult>> Find(ProductFilter? filter);
    Task<ProductResult?> Create(ProductRequest entity);
    Task<ProductResult?> Edit(Guid id, ProductRequest entity);
    Task<bool> Remove(Guid id);
    Task<bool> GenerateArticles(Guid productId, int amount);

}