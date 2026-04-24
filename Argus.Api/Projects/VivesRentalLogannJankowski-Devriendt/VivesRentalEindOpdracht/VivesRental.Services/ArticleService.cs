using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VivesRental.Enums;
using VivesRental.Model;
using VivesRental.Repository.Core;
using VivesRental.Services.Abstractions;
using VivesRental.Services.Extensions;
using VivesRental.Services.Mappers;
using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Requests;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services;

public class ArticleService : IArticleService
{
    private readonly VivesRentalDbContext _context;

    public ArticleService(VivesRentalDbContext context)
    {
        _context = context;
    }

    public Task<ArticleResult?> Get(Guid id)
    {
        return _context.Articles
            .Where(a => a.Id == id)
            .MapToResults()
            .FirstOrDefaultAsync();
    }
        
    public Task<List<ArticleResult>> Find(ArticleFilter? filter = null)
    {
        return _context.Articles
            .ApplyFilter(filter)
            .MapToResults()
            .ToListAsync();
    }
        
        
    public async Task<ArticleResult?> Create(ArticleRequest entity)
    {
        var article = new Article
        {
            ProductId = entity.ProductId,
            Status = entity.Status
        };

        _context.Articles.Add(article);

        await _context.SaveChangesAsync();

        return await Get(article.Id);
    }

    public async Task<bool> UpdateStatus(Guid articleId, ArticleStatus status)
    {
        //Get Product from unitOfWork
        var article = await _context.Articles
            .Where(a => a.Id == articleId)
            .FirstOrDefaultAsync();

        if (article == null)
        {
            return false;
        }

        //Only update the properties we want to update
        article.Status = status;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Removes one Article, Removes the ArticleReservations and disconnects OrderLines from the Article
    /// </summary>
    /// <param name="id">The id of the Article</param>
    /// <returns>True if the article was deleted</returns>
    public async Task<bool> Remove(Guid id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await RemoveInternal(id);
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task RemoveInternal(Guid id)
    {
        await ClearArticleByArticleId(id);
        _context.ArticleReservations.RemoveRange(_context.ArticleReservations.Where(ar => ar.ArticleId == id));

        var article = new Article { Id = id };
        _context.Articles.Attach(article);
        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();
    }
        

    private async Task ClearArticleByArticleId(Guid articleId)
    {
        var commandText = "UPDATE OrderLine SET ArticleId = null WHERE ArticleId = @ArticleId";
        var articleIdParameter = new SqlParameter("@ArticleId", articleId);

        await _context.Database.ExecuteSqlRawAsync(commandText, articleIdParameter);
    }

}