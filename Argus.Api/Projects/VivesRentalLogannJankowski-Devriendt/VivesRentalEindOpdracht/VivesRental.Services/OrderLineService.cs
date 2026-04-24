using Microsoft.EntityFrameworkCore;
using VivesRental.Repository.Core;
using VivesRental.Services.Abstractions;
using VivesRental.Services.Extensions;
using VivesRental.Services.Mappers;
using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services;

public class OrderLineService : IOrderLineService
{
    private readonly VivesRentalDbContext _context;

    public OrderLineService(VivesRentalDbContext context)
    {
        _context = context;
    }

    public Task<OrderLineResult?> Get(Guid id)
    {
        return _context.OrderLines
            .Where(ol => ol.Id == id)
            .MapToResults()
            .FirstOrDefaultAsync();
    }

    public Task<List<OrderLineResult>> Find(OrderLineFilter? filter = null)
    {
        return _context.OrderLines
            .ApplyFilter(filter)
            .MapToResults()
            .ToListAsync();
    }

    public async Task<bool> Rent(Guid orderId, Guid articleId)
    {
        var fromDateTime = DateTime.Now;
        var articleFilter = new ArticleFilter
        {
            AvailableFromDateTime = fromDateTime
        };
        var article = await _context.Articles
            .Include(a => a.Product)
            .Where(a => a.Id == articleId)
            .ApplyFilter(articleFilter)
            .SingleOrDefaultAsync();

        if (article == null)
        {
            //Article does not exist or is not available.
            return false;
        }

        var orderLine = article.CreateOrderLine(orderId);

        _context.OrderLines.Add(orderLine);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> Rent(Guid orderId, IList<Guid> articleIds)
    {
        var articleFilter = new ArticleFilter
        {
            ArticleIds = articleIds,
            AvailableFromDateTime = DateTime.Now
        };
            
        var articles = await _context.Articles
            .Include(a => a.Product) //Needs include for the CreateOrderLine extension method.
            .ApplyFilter(articleFilter)
            .ToListAsync();

        //If the amount of articles is not the same as the requested ids, some articles are not available anymore
        if (articleIds.Count != articles.Count)
        {
            return false;
        }

        foreach (var article in articles)
        {
            var orderLine = article.CreateOrderLine(orderId);
            _context.OrderLines.Add(orderLine);
        }

        var numberOfObjectsUpdated = await _context.SaveChangesAsync();
        return numberOfObjectsUpdated > 0;
    }

    /// <summary>
    /// Returns a rented article
    /// </summary>
    /// <param name="orderLineId"></param>
    /// <param name="returnedAt"></param>
    /// <returns></returns>
    public async Task<bool> Return(Guid orderLineId, DateTime returnedAt)
    {
        var orderLine = await _context.OrderLines
            .Where(ol => ol.Id == orderLineId)
            .FirstOrDefaultAsync();

        if (orderLine == null)
        {
            return false;
        }

        if (returnedAt == DateTime.MinValue)
        {
            return false;
        }

        if (orderLine.ReturnedAt.HasValue)
        {
            return false;
        }

        orderLine.ReturnedAt = returnedAt;

        await _context.SaveChangesAsync();
        return true;
    }
}