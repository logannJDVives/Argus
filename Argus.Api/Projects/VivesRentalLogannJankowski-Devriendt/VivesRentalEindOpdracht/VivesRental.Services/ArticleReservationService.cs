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

public class ArticleReservationService : IArticleReservationService
{
    private readonly VivesRentalDbContext _context;

    public ArticleReservationService(VivesRentalDbContext context)
    {
        _context = context;
    }
    
    public Task<ArticleReservationResult?> Get(Guid id)
    {
        return _context.ArticleReservations
            .Where(ar => ar.Id == id)
            .MapToResults()
            .FirstOrDefaultAsync();
    }
        
    public Task<List<ArticleReservationResult>> Find(ArticleReservationFilter? filter = null)
    {
        return _context.ArticleReservations
            .ApplyFilter(filter)
            .MapToResults()
            .ToListAsync();
    }
        
    public async Task<ArticleReservationResult?> Create(ArticleReservationRequest request)
    {
        request.FromDateTime ??= DateTime.Now;
        request.UntilDateTime ??= DateTime.Now.AddDays(7);

        // Haal artikel en gerelateerde data op
        var article = await _context.Articles
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId);

        if (article == null)
        {
            return null;
        }

        // Haal klant op
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer == null)
        {
            return null;
        }

        // Check of artikel beschikbaar is (niet al verhuurd in deze periode)
        var isArticleAvailable = await IsArticleAvailable(
            request.ArticleId, 
            request.FromDateTime.Value, 
            request.UntilDateTime.Value);

        if (!isArticleAvailable)
        {
            return null; // Artikel is niet beschikbaar
        }

        // Maak reservering aan
        var articleReservation = new ArticleReservation
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ArticleId = request.ArticleId,
            FromDateTime = request.FromDateTime.Value,
            UntilDateTime = request.UntilDateTime.Value,
            Status = ReservationStatus.Approved, // Direct goedgekeurd
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };

        _context.ArticleReservations.Add(articleReservation);

        // Maak automatisch een Order aan
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CustomerFirstName = customer.FirstName,
            CustomerLastName = customer.LastName,
            CustomerEmail = customer.Email,
            CustomerPhoneNumber = customer.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        var orderLine = new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ArticleId = article.Id,
            ProductName = article.Product.Name,
            ProductDescription = article.Product.Description,
            RentedAt = request.FromDateTime.Value,
            ExpiresAt = request.UntilDateTime.Value
        };

        _context.Orders.Add(order);
        _context.OrderLines.Add(orderLine);

        await _context.SaveChangesAsync();

        return await Get(articleReservation.Id);
    }

    /// <summary>
    /// Controleert of een artikel beschikbaar is voor een bepaalde periode
    /// </summary>
    private async Task<bool> IsArticleAvailable(Guid articleId, DateTime from, DateTime until)
    {
        // Check of er al een actieve reservering is voor dit artikel in deze periode
        var hasOverlappingReservation = await _context.ArticleReservations
            .AnyAsync(r => 
                r.ArticleId == articleId &&
                r.Status == ReservationStatus.Approved &&
                r.FromDateTime < until &&
                r.UntilDateTime > from);

        if (hasOverlappingReservation)
        {
            return false;
        }

        // Check of er een actieve order is (niet teruggebracht)
        var hasActiveOrder = await _context.OrderLines
            .AnyAsync(ol => 
                ol.ArticleId == articleId &&
                ol.ReturnedAt == null &&
                ol.RentedAt < until &&
                ol.ExpiresAt > from);

        return !hasActiveOrder;
    }

    public async Task<bool> Remove(Guid id)
    {
        var reservation = await _context.ArticleReservations
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            return false;
        }

        _context.ArticleReservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return true;
    }
}