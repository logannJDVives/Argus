using VivesRental.Model;
using VivesRental.Model.Extensions;
using VivesRental.Services.Model.Filters;

namespace VivesRental.Services.Extensions;

public static class FilterExtensions
{
    public static IQueryable<Article> ApplyFilter(this IQueryable<Article> query, ArticleFilter? filter)
    {
        if (filter == null)
        {
            return query;
        }

        if (filter.ArticleIds != null && filter.ArticleIds.Any())
        {
            query = query.Where(a => filter.ArticleIds.Contains(a.Id));
        }

        if (filter.ProductId.HasValue)
        {
            query = query.Where(a => a.ProductId == filter.ProductId);
        }
            
        if (filter.RentedByCustomerId.HasValue)
        {
            query = query.Where(a => a.OrderLines.Any(ol => ol.Order.CustomerId == filter.RentedByCustomerId));
        }

        if (filter.ReservedByCustomerId.HasValue)
        {
            query = query.Where(a => a.ArticleReservations.Any(ar => ar.CustomerId == filter.RentedByCustomerId));
        }

        if(filter.AvailableFromDateTime.HasValue || filter.AvailableUntilDateTime.HasValue)
        {
            query = query.Where(ArticleExtensions.IsAvailable(filter.AvailableFromDateTime, filter.AvailableUntilDateTime));
        }

        if (filter.RentedFromDateTime.HasValue || filter.RentedUntilDateTime.HasValue)
        {
            query = query.Where(ArticleExtensions.IsRented(filter.RentedFromDateTime,
                filter.RentedUntilDateTime));
        }
        if (filter.ReservedFromDateTime.HasValue || filter.ReservedUntilDateTime.HasValue)
        {
            query = query.Where(ArticleExtensions.IsReserved(filter.ReservedFromDateTime,
                filter.ReservedUntilDateTime));
        }
        return query;
    }

    public static IQueryable<ArticleReservation> ApplyFilter(this IQueryable<ArticleReservation> query, ArticleReservationFilter? filter)
    {
        if (filter == null)
        {
            return query;
        }

        if (filter.ArticleId.HasValue)
        {
            query = query.Where(a => a.ArticleId == filter.ArticleId);
        }

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(a => a.CustomerId == filter.CustomerId);
        }

        return query;
    }

    public static IQueryable<Customer> ApplyFilter(this IQueryable<Customer> query, CustomerFilter? filter)
    {
        if (filter == null)
        {
            return query;
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(c =>
                c.FirstName.ToLowerInvariant().Contains(filter.Search.ToLowerInvariant())
                || c.LastName.ToLowerInvariant().Contains(filter.Search.ToLowerInvariant()));
        }

        return query;
    }

    public static IQueryable<Order> ApplyFilter(this IQueryable<Order> query, OrderFilter? filter)
    {
        if (filter == null)
        {
            return query;
        }

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == filter.CustomerId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(c =>
                c.CustomerFirstName.ToLowerInvariant().Contains(filter.Search.ToLowerInvariant())
                || c.CustomerLastName.ToLowerInvariant().Contains(filter.Search.ToLowerInvariant())
                || c.CustomerEmail.ToLowerInvariant().Contains(filter.Search.ToLowerInvariant())
                || c.CustomerPhoneNumber.ToLowerInvariant().Contains(filter.Search.ToLowerInvariant()));
        }

        return query;
    }

    public static IQueryable<OrderLine> ApplyFilter(this IQueryable<OrderLine> query, OrderLineFilter? filter)
    {
        if (filter == null)
        {
            return query;
        }

        if (filter.OrderId.HasValue)
        {
            query = query.Where(ol => ol.OrderId == filter.OrderId);
        }

        return query;
    }

    public static IQueryable<Product> ApplyFilter(this IQueryable<Product> query, ProductFilter? filter)
    {
        if (filter == null)
        {
            return query;
        }

        query = query.Where(ProductExtensions.IsAvailable(filter.AvailableFromDateTime,
            filter.AvailableUntilDateTime));

        return query;
    }
}