using Microsoft.VisualStudio.TestTools.UnitTesting;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Services.Tests;

[TestClass]
public class OrderServiceTests
{
    [TestMethod]
    public async Task Get_Returns_Order_With_One_NumberOfOrderLines()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Get_Returns_Order_With_One_NumberOfOrderLines");

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

        var orderService = new OrderService(context);

        //Act
        var result = await orderService.Get(order.Id);

        //Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.NumberOfOrderLines);
    }

    [TestMethod]
    public async Task Get_Returns_Order_With_Multiple_NumberOfOrderLines()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Get_Returns_Order_With_Multiple_NumberOfOrderLines");

        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article1 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article1);
        var article2 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article2);
        var article3 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article3);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        var orderLine1 = OrderLineFactory.CreateValidEntity(order, article1);
        context.OrderLines.Add(orderLine1);
        var orderLine2 = OrderLineFactory.CreateValidEntity(order, article2);
        context.OrderLines.Add(orderLine2);
        var orderLine3 = OrderLineFactory.CreateValidEntity(order, article3);
        context.OrderLines.Add(orderLine3);
        await context.SaveChangesAsync();

        var orderService = new OrderService(context);

        //Act
        var result = await orderService.Get(order.Id);

        //Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.NumberOfOrderLines);
    }
}