using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Requests;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services.Abstractions;

public interface IArticleReservationService
{
    Task<ArticleReservationResult?> Get(Guid id);
    Task<List<ArticleReservationResult>> Find(ArticleReservationFilter? filter = null);
    Task<ArticleReservationResult?> Create(ArticleReservationRequest entity);
    Task<bool> Remove(Guid id);
}