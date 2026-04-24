using System.Linq.Expressions;
using VivesRental.Model;
using VivesRental.Model.Extensions;
using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services.Mappers;

public static class ProjectionExpressions
{
    public static Expression<Func<Article, ArticleResult>> ProjectToArticleResult()
    {
        return entity => new ArticleResult
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = entity.Product.Name,
            Status = entity.Status
        };
    }

    public static Expression<Func<ArticleReservation, ArticleReservationResult>> ProjectToArticleReservationResult()
    {
        return entity => new ArticleReservationResult
        {
            Id = entity.Id,
            ArticleId = entity.ArticleId,
            ArticleStatus = entity.Article.Status,
            FromDateTime = entity.FromDateTime,
            UntilDateTime = entity.UntilDateTime,
            CustomerId = entity.CustomerId,
            CustomerFirstName = entity.Customer.FirstName,
            CustomerLastName = entity.Customer.LastName,
            CustomerEmail = entity.Customer.Email,
            ProductName = entity.Article.Product.Name,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ProcessedAt = entity.ProcessedAt,
            RejectionReason = entity.RejectionReason
        };
    }

    public static Expression<Func<Customer, CustomerResult>> ProjectToCustomerResult()
    {
        return entity => new CustomerResult
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            NumberOfOrders = entity.Orders.Count,
            NumberOfPendingOrders = entity.Orders.Count(o => o.OrderLines.Any(ol => !ol.ReturnedAt.HasValue))
        };
    }

    public static Expression<Func<Order, OrderResult>> ProjectToOrderResult()
    {
        return entity => new OrderResult
        {
            Id = entity.Id,
            CustomerFirstName = entity.CustomerFirstName,
            CustomerLastName = entity.CustomerLastName,
            CustomerEmail = entity.CustomerEmail,
            CustomerPhoneNumber = entity.CustomerPhoneNumber,
            CreatedAt = entity.CreatedAt,
            CustomerId = entity.CustomerId,
            ReturnedAt = entity.OrderLines.All(ol => ol.ReturnedAt.HasValue)
                ? entity.OrderLines.Max(ol => ol.ReturnedAt)
                : null,
            NumberOfOrderLines = entity.OrderLines.Count()
        };
    }

    public static Expression<Func<OrderLine, OrderLineResult>> ProjectToOrderLineResult()
    {
        return entity => new OrderLineResult
        {
            Id = entity.Id,
            ArticleId = entity.ArticleId,
            OrderId = entity.OrderId,
            ProductName = entity.ProductName,
            ProductDescription = entity.ProductDescription,
            RentedAt = entity.RentedAt,
            ExpiresAt = entity.ExpiresAt,
            ReturnedAt = entity.ReturnedAt
        };
    }

    public static Expression<Func<Product, ProductResult>> ProjectToProductResult(ProductFilter? filter = null)
    {
        filter ??= new ProductFilter();

        return entity => new ProductResult
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Manufacturer = entity.Manufacturer,
            Publisher = entity.Publisher,
            RentalExpiresAfterDays = entity.RentalExpiresAfterDays,
            NumberOfArticles = entity.Articles.Count,
            NumberOfAvailableArticles = entity.Articles
                .AsQueryable()
                .Count(ArticleExtensions.IsAvailable(filter.AvailableFromDateTime, filter.AvailableUntilDateTime))
        };
    }
        
}