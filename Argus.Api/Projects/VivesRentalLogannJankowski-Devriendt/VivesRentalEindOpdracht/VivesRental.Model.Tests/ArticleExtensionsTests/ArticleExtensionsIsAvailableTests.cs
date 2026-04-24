using VivesRental.Model.Extensions;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Model.Tests.ArticleExtensionsTests;

public class ArticleExtensionsIsAvailableTests
{
    [Fact]
    public void IsAvailable_Returns_True_When_Available()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);

        var isAvailableFunc = ArticleExtensions.IsAvailable().Compile();

        //Act
        var result = isAvailableFunc(article);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAvailable_Returns_True_When_Available_OnlyFromDate()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);

        var fromDate = new DateTime(2022, 1, 1);

        var isAvailableFunc = ArticleExtensions.IsAvailable(fromDate).Compile();

        //Act
        var result = isAvailableFunc(article);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAvailable_Returns_False_When_Rented()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var order = OrderFactory.CreateValidEntity(customer);
        OrderLineFactory.CreateValidEntity(order, article);

        var isAvailableFunc = ArticleExtensions.IsAvailable().Compile();

        //Act
        var result = isAvailableFunc(article);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAvailable_Returns_False_When_Reserved_In_Period()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        ArticleReservationFactory.CreateValidEntity(customer, article,
            new DateTime(2022, 1, 1), new DateTime(2022, 1, 10));

        var periodFrom = new DateTime(2021, 12, 1);
        var periodUntil = new DateTime(2022, 1, 5);

        var isAvailableFunc = ArticleExtensions.IsAvailable(periodFrom, periodUntil).Compile();

        //Act
        var result = isAvailableFunc(article);

        //Assert
        Assert.False(result);
    }
}