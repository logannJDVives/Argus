using System.Linq.Expressions;

namespace VivesRental.Model.Extensions;

public static class ProductExtensions
{
    public static Expression<Func<Product, bool>> IsAvailable(
        DateTime? fromDateTime = null, 
        DateTime? untilDateTime = null)
    {
        return p => p.Articles
            .AsQueryable()
            .Any(ArticleExtensions.IsAvailable(fromDateTime, untilDateTime));
    }

}