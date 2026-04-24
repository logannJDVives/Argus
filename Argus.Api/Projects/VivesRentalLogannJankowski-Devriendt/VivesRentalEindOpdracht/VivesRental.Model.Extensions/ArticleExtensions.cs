using System.Linq.Expressions;
using VivesRental.Enums;

namespace VivesRental.Model.Extensions;

public static class ArticleExtensions
{
    //checks die uitgevoerd / gecontroleerd worden. (foutafhandeling basically)
    public static Expression<Func<Article, bool>> IsAvailable(DateTime? fromDateTime = null, DateTime? untilDateTime = null)
    {
        Expression<Func<Article, bool>> expression = a => a.Status == ArticleStatus.Normal;
        return expression
            .And(IsReserved(fromDateTime, untilDateTime).Not())
            .And(IsRented(fromDateTime, untilDateTime).Not());
    }

    public static Expression<Func<Article, bool>> IsReserved(DateTime? fromDateTime = null, DateTime? untilDateTime = null)
    {
        return a => a.ArticleReservations.AsQueryable()
            .Any(ArticleReservationExtensions.IsReserved(fromDateTime, untilDateTime));
    }

    public static Expression<Func<Article, bool>> IsRented(DateTime? fromDateTime = null, DateTime? untilDateTime = null)
    {
        return a => a.OrderLines.Any(ol => !ol.ReturnedAt.HasValue
                                           && (!fromDateTime.HasValue || ol.RentedAt >= fromDateTime.Value)
                                           && (!untilDateTime.HasValue || ol.RentedAt <= untilDateTime.Value));
    }
}