using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VivesRental.Model;
using VivesRental.Repository.Core;
using VivesRental.Services.Abstractions;
using VivesRental.Services.Extensions;
using VivesRental.Services.Mappers;
using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Requests;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services;

public class ProductService : IProductService
{
    private readonly VivesRentalDbContext _context;
    public ProductService(VivesRentalDbContext context)
    {
        _context = context;
    }


    public Task<ProductResult?> Get(Guid id)
    {
        return _context.Products
            .Where(p => p.Id == id)
            .MapToResults()
            .FirstOrDefaultAsync();
    }

    public Task<List<ProductResult>> Find(ProductFilter? filter = null)
    {

        return _context.Products
            .ApplyFilter(filter)
            .MapToResults(filter)
            .ToListAsync();
    }

    public async Task<ProductResult?> Create(ProductRequest entity)
    {
        var product = new Product
        {
            Name = entity.Name,
            Description = entity.Description,
            Manufacturer = entity.Manufacturer,
            Publisher = entity.Publisher,
            RentalExpiresAfterDays = entity.RentalExpiresAfterDays
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return await Get(product.Id);
    }

    public async Task<ProductResult?> Edit(Guid id, ProductRequest entity)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return null;
        }
            
        product.Name = entity.Name;
        product.Description = entity.Description;
        product.Manufacturer = entity.Manufacturer;
        product.Publisher = entity.Publisher;
        product.RentalExpiresAfterDays = entity.RentalExpiresAfterDays;

        await _context.SaveChangesAsync();

        return await Get(product.Id);
    }

    /// <summary>
    /// Removes one Product, removes ArticleReservations, removes all linked articles and disconnects OrderLines from articles
    /// </summary>
    /// <param name="id">The id of the Product</param>
    /// <returns>True if the product was deleted</returns>
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
        await ClearArticleByProductId(id);
        _context.ArticleReservations.RemoveRange(
            _context.ArticleReservations.Where(a => a.Article.ProductId == id));
        _context.Articles.RemoveRange(_context.Articles.Where(a => a.ProductId == id));

        //Remove product
        var product = new Product { Id = id };
        _context.Products.Attach(product);
        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Adds a number of articles with Normal status to the Product.
    /// This is limited to maximum 10.000
    /// </summary>
    /// <returns>True if articles are added</returns>
    public async Task<bool> GenerateArticles(Guid productId, int amount)
    {
        if (amount <= 0 || amount > 10000) //Set a limit to 10K
        {
            return false;
        }

        for (int i = 0; i < amount; i++)
        {
            var article = new Article
            {
                ProductId = productId
            };
            _context.Articles.Add(article);
        }

        var numberOfObjectsUpdated = await _context.SaveChangesAsync();
        return numberOfObjectsUpdated > 0;
    }
        
    private async Task ClearArticleByProductId(Guid productId)
    {
        var commandText =
            "UPDATE OrderLine SET ArticleId = null from OrderLine inner join Article on Article.ProductId = @ProductId";
        var articleIdParameter = new SqlParameter("@ProductId", productId);

        await _context.Database.ExecuteSqlRawAsync(commandText, articleIdParameter);
    }
}