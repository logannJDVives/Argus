using System.Linq.Expressions;

namespace VivesRental.Model.Extensions;

public static class ArticleReservationExtensions
{
    public static Expression<Func<ArticleReservation, bool>> IsReserved(DateTime? fromDateTime = null, DateTime? untilDateTime = null)
    {
        return ar =>
            (untilDateTime.HasValue && ar.FromDateTime < untilDateTime || fromDateTime.HasValue && ar.FromDateTime <
                fromDateTime.Value.AddDays(ar.Article.Product.RentalExpiresAfterDays)
            ) //If we do not have an UntilDateTime, just add the expiry days to the FromDate
            && ar.UntilDateTime > fromDateTime;
    }
}