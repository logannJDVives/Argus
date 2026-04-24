using Microsoft.VisualStudio.TestTools.UnitTesting;
using VivesRental.Services.Mappers;
using VivesRental.Services.Model.Filters;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Services.Tests.ProjectionExpressionTests;

[TestClass]
public class ProjectToProductResultTests
{
    [TestMethod]
    public void Maps_A_Product_To_ProductResult()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();
        var projectionFunction = ProjectionExpressions.ProjectToProductResult().Compile();

        //Act
        var productResult = projectionFunction(product);

        //Assert
        Assert.AreEqual(product.Id, productResult.Id);
    }

    [TestMethod]
    public void Maps_A_Product_To_ProductResult_With_Articles()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();
        ArticleFactory.CreateValidEntity(product);
        var projectionFunction = ProjectionExpressions.ProjectToProductResult().Compile();

        //Act
        var productResult = projectionFunction(product);

        //Assert
        Assert.AreEqual(1, productResult.NumberOfArticles);
    }

    [TestMethod]
    public void Maps_A_Product_To_ProductResult_With_Available_Articles()
    {
        //Arrange
        var product = ProductFactory.CreateValidEntity();
        ArticleFactory.CreateValidEntity(product);
        var projectionFunction = ProjectionExpressions.ProjectToProductResult().Compile();

        //Act
        var productResult = projectionFunction(product);

        //Assert
        Assert.AreEqual(1, productResult.NumberOfAvailableArticles);
    }

    [TestMethod]
    public void Maps_A_Product_To_ProductResult_With_UnAvailable_Articles_Inside_Reservation_Period()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var reservedFrom = new DateTime(2022, 1, 1);
        var reservedUntil = new DateTime(2022, 2, 1);
        ArticleReservationFactory.CreateValidEntity(customer, article, reservedFrom, reservedUntil);

        var filter = new ProductFilter
        {
            AvailableFromDateTime = new DateTime(2021, 12, 1),
            AvailableUntilDateTime = new DateTime(2022, 1, 10)
        };
        var projectionFunction = ProjectionExpressions.ProjectToProductResult(filter).Compile();

        //Act
        var productResult = projectionFunction(product);

        //Assert
        Assert.AreEqual(0, productResult.NumberOfAvailableArticles);
    }

    [TestMethod]
    public void Maps_A_Product_To_ProductResult_With_Available_Articles_Outside_Reservation_Period()
    {
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        var product = ProductFactory.CreateValidEntity();
        var article = ArticleFactory.CreateValidEntity(product);
        var reservedFrom = new DateTime(2022, 1, 1);
        var reservedUntil = new DateTime(2022, 2, 1);
        ArticleReservationFactory.CreateValidEntity(customer, article, reservedFrom, reservedUntil);

        var filter = new ProductFilter
        {
            AvailableFromDateTime = new DateTime(2021, 11, 1),
            AvailableUntilDateTime = new DateTime(2021, 12, 10)
        };
        var projectionFunction = ProjectionExpressions.ProjectToProductResult(filter).Compile();

        //Act
        var productResult = projectionFunction(product);

        //Assert
        Assert.AreEqual(1, productResult.NumberOfAvailableArticles);
    }
}
