using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;

namespace VivesRental.Sdk.Interfaces;

public interface IArticleReservationSdk
{
    Task<IList<ArticleReservationResult>> FindAsync();
    Task<ArticleReservationResult?> GetAsync(Guid id);
    Task<ArticleReservationResult?> CreateAsync(ArticleReservationRequest request);
    Task<bool> DeleteAsync(Guid id);
}
