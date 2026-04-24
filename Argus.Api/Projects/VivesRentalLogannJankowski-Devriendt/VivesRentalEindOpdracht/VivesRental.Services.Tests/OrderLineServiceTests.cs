using Microsoft.VisualStudio.TestTools.UnitTesting;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Services.Tests;

[TestClass]
public class OrderLineServiceTests
{
    [TestMethod]
    public async Task Rent_Returns_True_When_Rented()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance(nameof(Rent_Returns_True_When_Rented));

        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance(nameof(Rent_Returns_True_When_Rented));
        var orderLineService = new OrderLineService(serviceContext);

        //Act
        var result = await orderLineService.Rent(order.Id, article.Id);

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Rent_Returns_False_When_Article_Does_Not_Exist()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance(nameof(Rent_Returns_False_When_Article_Does_Not_Exist));

        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance(nameof(Rent_Returns_False_When_Article_Does_Not_Exist));
        var orderLineService = new OrderLineService(serviceContext);

        //Act
        var result = await orderLineService.Rent(order.Id, Guid.NewGuid());

        //Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Rent_IdList_Returns_True_When_One_Rented()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance(nameof(Rent_IdList_Returns_True_When_One_Rented));

        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance(nameof(Rent_IdList_Returns_True_When_One_Rented));
        var orderLineService = new OrderLineService(serviceContext);
        var idList = new List<Guid> { article.Id };

        //Act
        var result = await orderLineService.Rent(order.Id, idList);

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Rent_IdList_Returns_False_When_One_Rented_And_Does_Not_Exist()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance(nameof(Rent_IdList_Returns_False_When_One_Rented_And_Does_Not_Exist));

        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance(nameof(Rent_IdList_Returns_False_When_One_Rented_And_Does_Not_Exist));
        var orderLineService = new OrderLineService(serviceContext);
        var idList = new List<Guid> { Guid.NewGuid() };

        //Act
        var result = await orderLineService.Rent(order.Id, idList);

        //Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Rent_IdList_Returns_True_When_Multiple_Rented()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance(nameof(Rent_IdList_Returns_True_When_Multiple_Rented));

        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article1 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article1);
        var article2 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article2);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance(nameof(Rent_IdList_Returns_True_When_Multiple_Rented));
        var orderLineService = new OrderLineService(serviceContext);
        var idList = new List<Guid> {article1.Id, article2.Id};

        //Act
        var result = await orderLineService.Rent(order.Id, idList);

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Rent_IdList_Returns_False_When_Multiple_Rented_And_One_Does_Not_Exist()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance(nameof(Rent_IdList_Returns_False_When_Multiple_Rented_And_One_Does_Not_Exist));

        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article1 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article1);
        var article2 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article2);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance(nameof(Rent_IdList_Returns_False_When_Multiple_Rented_And_One_Does_Not_Exist));
        var orderLineService = new OrderLineService(serviceContext);
        var idList = new List<Guid> { article1.Id, article2.Id, Guid.NewGuid() };

        //Act
        var result = await orderLineService.Rent(order.Id, idList);

        //Assert
        Assert.IsFalse(result);
    }
}