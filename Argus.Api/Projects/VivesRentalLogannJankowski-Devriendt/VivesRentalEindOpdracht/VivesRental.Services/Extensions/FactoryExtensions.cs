using VivesRental.Model;

namespace VivesRental.Services.Extensions;

public static class FactoryExtensions
{
    public static OrderLine CreateOrderLine(this Article article, Guid orderId)
    {
        return new OrderLine
        {
            ArticleId = article.Id,
            OrderId = orderId,
            ProductName = article.Product.Name,
            ProductDescription = article.Product.Description,
            ExpiresAt = DateTime.Now.AddDays(article.Product.RentalExpiresAfterDays),
            RentedAt = DateTime.Now
        };
    }
}