using Microsoft.VisualStudio.TestTools.UnitTesting;
using VivesRental.Enums;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Services.Tests;

[TestClass]
public class ArticleServiceTests
{
    [TestMethod]
    public async Task Remove_Deletes_Article()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Remove_Deletes_Article");
            
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("Remove_Deletes_Article");
        var articleService = new ArticleService(serviceContext);

        //Act
        var result = await articleService.Remove(article.Id);

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Remove_Deletes_Article_With_OrderLines()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Remove_Deletes_Article_With_OrderLines");
            
        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);
        context.OrderLines.Add(orderLine);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("Remove_Deletes_Article_With_OrderLines");
        var articleService = new ArticleService(serviceContext);

        //Act
        var result = await articleService.Remove(article.Id);

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task UpdateStatus_Returns_True()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("UpdateStatus_Returns_True");

        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("UpdateStatus_Returns_True");
        var articleService = new ArticleService(serviceContext);

        //Act
        var result = await articleService.UpdateStatus(article.Id, ArticleStatus.Broken);

        //Assert
        Assert.IsTrue(result);
    }
}