using VivesRental.Model;

namespace VivesRental.Tests.Data.Factories;

public static class ArticleReservationFactory
{
    public static ArticleReservation CreateValidEntity(Customer customer, Article article, DateTime fromDateTime, DateTime untilDateTime)
    {
        var articleReservation = new ArticleReservation
        {
            CustomerId = customer.Id,
            Customer = customer,
            ArticleId = article.Id,
            Article = article,
            FromDateTime = fromDateTime,
            UntilDateTime = untilDateTime
        };
        customer.ArticleReservations.Add(articleReservation);
        article.ArticleReservations.Add(articleReservation);

        return articleReservation;
    }

    public static ArticleReservation CreateInvalidEntity()
    {
        return new ArticleReservation();
    }
}