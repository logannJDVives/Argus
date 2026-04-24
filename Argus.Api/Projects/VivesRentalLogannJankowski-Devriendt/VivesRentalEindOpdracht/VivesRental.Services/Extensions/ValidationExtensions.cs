using VivesRental.Model;

namespace VivesRental.Services.Extensions;
public static class ValidationExtensions
{
    public static bool IsValid(this Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return false;
        }

        return true;
    }

    public static bool IsValid(this Article article)
    {
        if (article.ProductId == Guid.Empty)
        {
            return false;
        }
            
        return true;
    }

    public static bool IsValid(this Order order)
    {
        if (order.CustomerId == Guid.Empty)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(order.CustomerFirstName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(order.CustomerLastName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(order.CustomerEmail))
        {
            return false;
        }

        if (order.CreatedAt == DateTime.MinValue)
        {
            return false;
        }

        return true;
    }

    public static bool IsValid(this OrderLine orderLine)
    {
        if (orderLine.OrderId == Guid.Empty)
        {
            return false;
        }

        if (orderLine.ArticleId == Guid.Empty)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(orderLine.ProductName))
        {
            return false;
        }

        if (orderLine.RentedAt == DateTime.MinValue)
        {
            return false;
        }

        if (orderLine.ExpiresAt == DateTime.MinValue)
        {
            return false;
        }

        return true;
    }

    public static bool IsValid(this Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.FirstName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(customer.LastName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            return false;
        }

        return true;
    }

    public static bool IsValid(this ArticleReservation articleReservation)
    {
        if (articleReservation.ArticleId == Guid.Empty)
        {
            return false;
        }

        if (articleReservation.CustomerId == Guid.Empty)
        {
            return false;
        }

        //Do not allow an Until date before From date
        if (articleReservation.UntilDateTime < articleReservation.FromDateTime)
        {
            return false;
        }

        return true;
    }
}