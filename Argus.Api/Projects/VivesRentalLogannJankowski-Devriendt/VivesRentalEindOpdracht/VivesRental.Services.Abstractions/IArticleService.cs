using VivesRental.Enums;
using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Requests;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services.Abstractions;

public interface IArticleService
{
    Task<ArticleResult?> Get(Guid id);
        
    Task<List<ArticleResult>> Find(ArticleFilter? filter = null);
        
    Task<ArticleResult?> Create(ArticleRequest entity);
       
    Task<bool> UpdateStatus(Guid articleId, ArticleStatus status);
    Task<bool> Remove(Guid id);
        
}