using VivesRental.Enums;
using VivesRental.Model;

namespace VivesRental.Tests.Data.Factories;

public static class ArticleFactory
{
    public static Article CreateValidEntity(Product product)
    {
        var article = new Article
        {
            ProductId = product.Id,
            Product = product,
            Status = ArticleStatus.Normal
        };

        product.Articles.Add(article);

        return article;
    }

    public static Article CreateInvalidEntity()
    {
        return new Article
        {
            Status = ArticleStatus.InRepair
        };
    }
}