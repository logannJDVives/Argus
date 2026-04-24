using VivesRental.Model.Extensions;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Model.Tests.ArticleExtensionsTests;

public class ArticleExtensionsIsRentedTests
{
    [Fact]
    public void IsRented_Returns_False_When_Not_Rented()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);

        var isRentedFunc = ArticleExtensions.IsRented().Compile();

        //Act
        var result = isRentedFunc(article);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRented_Returns_False_When_Not_Rented_Only_FromDate()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);

        var rentedFrom = new DateTime(2022, 1, 1);
        var isRentedFunc = ArticleExtensions.IsRented(rentedFrom).Compile();

        //Act
        var result = isRentedFunc(article);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRented_Returns_True_When_Rented()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var order = OrderFactory.CreateValidEntity(customer);
        OrderLineFactory.CreateValidEntity(order, article);

        var isRentedFunc = ArticleExtensions.IsRented().Compile();

        //Act
        var result = isRentedFunc(article);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRented_Returns_False_Outside_Rented_Period()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var order = OrderFactory.CreateValidEntity(customer);
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);

        orderLine.RentedAt = new DateTime(2022, 01, 01);

        var fromDateTime = new DateTime(2021, 1, 1);
        var untilDateTime = new DateTime(2021, 2, 1);

        var isRentedFunc = ArticleExtensions.IsRented(fromDateTime, untilDateTime).Compile();

        //Act
        var result = isRentedFunc(article);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRented_Returns_True_Inside_Rented_Period()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var order = OrderFactory.CreateValidEntity(customer);
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);

        orderLine.RentedAt = new DateTime(2022, 01, 01);

        var fromDateTime = new DateTime(2022, 1, 1);
        var untilDateTime = new DateTime(2023, 2, 1);

        var isRentedFunc = ArticleExtensions.IsRented(fromDateTime, untilDateTime).Compile();

        //Act
        var result = isRentedFunc(article);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRented_Returns_False_Outside_Rented_Period_And_Returned()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var order = OrderFactory.CreateValidEntity(customer);
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);

        orderLine.RentedAt = new DateTime(2022, 01, 01);
        orderLine.ReturnedAt = new DateTime(2022, 1, 10);

        var fromDateTime = new DateTime(2021, 1, 1);
        var untilDateTime = new DateTime(2021, 2, 1);

        var isRentedFunc = ArticleExtensions.IsRented(fromDateTime, untilDateTime).Compile();

        //Act
        var result = isRentedFunc(article);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRented_Returns_False_Inside_Rented_Period_And_Returned()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var order = OrderFactory.CreateValidEntity(customer);
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);

        orderLine.RentedAt = new DateTime(2022, 01, 01);
        orderLine.ReturnedAt = new DateTime(2022, 1, 10);

        var fromDateTime = new DateTime(2022, 1, 1);
        var untilDateTime = new DateTime(2023, 2, 1);

        var isRentedFunc = ArticleExtensions.IsRented(fromDateTime, untilDateTime).Compile();

        //Act
        var result = isRentedFunc(article);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void IsRented_Returns_True_Inside_Rented_Period_And_After_Returned()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var order = OrderFactory.CreateValidEntity(customer);
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);

        orderLine.RentedAt = new DateTime(2022, 01, 01);
        orderLine.ReturnedAt = new DateTime(2022, 1, 10);

        var fromDateTime = new DateTime(2022, 1, 11);
        var untilDateTime = new DateTime(2023, 2, 1);

        var isRentedFunc = ArticleExtensions.IsRented(fromDateTime, untilDateTime).Compile();

        //Act
        var result = isRentedFunc(article);

        //Assert
        Assert.False(result);
    }
}