using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Enums;

namespace VivesRental.Sdk.Interfaces;

public interface IArticleSdk
{
    Task<IList<ArticleResult>> FindAsync(Guid? productId = null);
    Task<ArticleResult?> GetAsync(Guid id);
    Task<ArticleResult?> GetFirstAvailableAsync(Guid productId);
    Task<ArticleResult?> CreateAsync(ArticleRequest request);
    Task<bool> UpdateStatusAsync(Guid id, ArticleStatus status);
    Task<bool> DeleteAsync(Guid id);
}
