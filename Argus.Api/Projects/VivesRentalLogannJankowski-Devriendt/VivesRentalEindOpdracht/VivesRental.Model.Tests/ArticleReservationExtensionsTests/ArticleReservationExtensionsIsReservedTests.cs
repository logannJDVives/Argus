using VivesRental.Model.Extensions;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Model.Tests.ArticleReservationExtensionsTests;

public class ArticleReservationExtensionsIsReservedTests
{
    [Fact]
    public void IsReserved_Returns_False_When_No_Period()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var reservedFrom = new DateTime(2022, 1, 1);
        var reservedUntil = new DateTime(2022, 2, 1);
        var articleReservation = ArticleReservationFactory.CreateValidEntity(customer, article, reservedFrom, reservedUntil);

        var isReserved = ArticleReservationExtensions.IsReserved().Compile();

        //Act
        var result = isReserved(articleReservation);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public void IsReserved_Returns_True_When_Reserved_In_Period()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var reservedFrom = new DateTime(2022, 1, 1);
        var reservedUntil = new DateTime(2022, 2, 1);
        var articleReservation = ArticleReservationFactory.CreateValidEntity(customer, article, reservedFrom, reservedUntil);

        var queryFrom = new DateTime(2021, 12, 1);
        var queryUntil = new DateTime(2022, 2, 1);
        var isReserved = ArticleReservationExtensions.IsReserved(queryFrom, queryUntil).Compile();

        //Act
        var result = isReserved(articleReservation);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public void IsReserved_Returns_False_When_Not_Reserved_In_Period()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var reservedFrom = new DateTime(2022, 1, 1);
        var reservedUntil = new DateTime(2022, 2, 1);
        var articleReservation = ArticleReservationFactory.CreateValidEntity(customer, article, reservedFrom, reservedUntil);

        var queryFrom = new DateTime(2021, 11, 1);
        var queryUntil = new DateTime(2021, 12, 1);
        var isReserved = ArticleReservationExtensions.IsReserved(queryFrom, queryUntil).Compile();

        //Act
        var result = isReserved(articleReservation);

        //Assert
        Assert.False(result);
    }
}