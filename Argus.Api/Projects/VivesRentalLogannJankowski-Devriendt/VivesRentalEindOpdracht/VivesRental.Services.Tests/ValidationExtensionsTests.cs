using Microsoft.VisualStudio.TestTools.UnitTesting;
using VivesRental.Services.Extensions;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Services.Tests;

[TestClass]
public class ValidationExtensionsTests
{
    [TestMethod]
    public void Product_IsValid_Returns_True_When_Valid()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();

        //Act
        var result = product.IsValid();

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Product_IsValid_Returns_False_When_Invalid()
    {
        //Arrange
        var product = ProductFactory.CreateInvalidEntity();

        //Act
        var result = product.IsValid();

        //Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Customer_IsValid_Returns_True_When_Valid()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();

        //Act
        var result = customer.IsValid();

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Customer_IsValid_Returns_False_When_Invalid()
    {
        //Arrange
        var customer = CustomerFactory.CreateInvalidEntity();

        //Act
        var result = customer.IsValid();

        //Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Article_IsValid_Returns_True_When_Valid()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();
        product.Id = Guid.NewGuid();
        var article = ArticleFactory.CreateValidEntity(product);
        article.Id = Guid.NewGuid();

        //Act
        var result = article.IsValid();

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Article_IsValid_Returns_False_When_Invalid()
    {
        //Arrange
        var article = ArticleFactory.CreateInvalidEntity();

        //Act
        var result = article.IsValid();

        //Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Order_IsValid_Returns_True_When_Valid()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        customer.Id = Guid.NewGuid();
        var order = OrderFactory.CreateValidEntity(customer);
        order.Id = Guid.NewGuid();

        //Act
        var result = order.IsValid();

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Order_IsValid_Returns_False_When_Invalid()
    {
        //Arrange
        var order = OrderFactory.CreateInvalidEntity();

        //Act
        var result = order.IsValid();

        //Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void OrderLine_IsValid_Returns_True_When_Valid()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        customer.Id = Guid.NewGuid();
        var order = OrderFactory.CreateValidEntity(customer);
        order.Id = Guid.NewGuid();
        var product = ProductFactory.CreateValidEntity();
        product.Id = Guid.NewGuid();
        var article = ArticleFactory.CreateValidEntity(product);
        article.Id = Guid.NewGuid();
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);
        orderLine.Id = Guid.NewGuid();

        //Act
        var result = orderLine.IsValid();

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void OrderLine_IsValid_Returns_False_When_Invalid()
    {
        //Arrange
        var order = OrderFactory.CreateInvalidEntity();

        //Act
        var result = order.IsValid();

        //Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ArticleReservation_IsValid_Returns_True_When_Valid()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        customer.Id = Guid.NewGuid();
        var order = OrderFactory.CreateValidEntity(customer);
        order.Id = Guid.NewGuid();
        var product = ProductFactory.CreateValidEntity();
        product.Id = Guid.NewGuid();
        var article = ArticleFactory.CreateValidEntity(product);
        article.Id = Guid.NewGuid();
        var entity = ArticleReservationFactory.CreateValidEntity(customer, article, DateTime.Now, DateTime.Now.AddDays(1));

        //Act
        var result = entity.IsValid();

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ArticleReservation_IsValid_Returns_False_When_Invalid()
    {
        //Arrange
        var entity = ArticleReservationFactory.CreateInvalidEntity();

        //Act
        var result = entity.IsValid();

        //Assert
        Assert.IsFalse(result);
    }
}