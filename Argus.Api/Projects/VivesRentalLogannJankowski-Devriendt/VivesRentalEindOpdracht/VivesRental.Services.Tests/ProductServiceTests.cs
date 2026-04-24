using Microsoft.VisualStudio.TestTools.UnitTesting;
using VivesRental.Services.Model.Filters;
using VivesRental.Tests.Data.Factories;

namespace VivesRental.Services.Tests;

[TestClass]
public class ProductServiceTests
{
    [TestMethod]
    public async Task Finds_A_List_Of_Products()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Finds_A_List_Of_Products");
        for (int i = 0; i < 10; i++)
        {
            var product = ProductFactory.CreateValidEntity();
            context.Products.Add(product);
        }
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("Finds_A_List_Of_Products");
        var sut = new ProductService(serviceContext);

        //Act
        var products = await sut.Find();

        //Assert
        Assert.AreEqual(10, products.Count);
    }

    [TestMethod]
    public async Task Finds_An_Empty_List_Of_Products()
    {
        //Arrange
        await using var serviceContext = DbContextFactory.CreateInstance("Finds_An_Empty_List_Of_Products");
        var sut = new ProductService(serviceContext);

        //Act
        var products = await sut.Find();

        //Assert
        Assert.AreEqual(0, products.Count);
    }

    [TestMethod]
    public async Task Gets_One_Product()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Gets_One_Product");
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("Gets_One_Product");
        var sut = new ProductService(serviceContext);

        //Act
        var serviceProduct = await sut.Get(product.Id);

        //Assert
        Assert.IsNotNull(serviceProduct);
        Assert.AreEqual(product.Id, serviceProduct.Id);
    }


    [TestMethod]
    public async Task Remove_Deletes_Product()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Remove_Deletes_Product");
            
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("Remove_Deletes_Product");
        var productService = new ProductService(serviceContext);

        //Act
        var result = await productService.Remove(product.Id);

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Remove_Deletes_Product_With_Articles()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Remove_Deletes_Product_With_Articles");
            
        var productToAdd = ProductFactory.CreateValidEntity();
        context.Products.Add(productToAdd);
        var article = ArticleFactory.CreateValidEntity(productToAdd);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("Remove_Deletes_Product_With_Articles");
        var productService = new ProductService(serviceContext);

        //Act
        var result = await productService.Remove(productToAdd.Id);

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Remove_Deletes_Product_With_Articles_And_OrderLines()
    {
        //Arrange
        await using var context = DbContextFactory.CreateInstance("Remove_Deletes_Product_With_Articles_And_OrderLines");
            
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

        await using var serviceContext = DbContextFactory.CreateInstance("Remove_Deletes_Product_With_Articles_And_OrderLines");
        var productService = new ProductService(serviceContext);

        //Act
        var result = await productService.Remove(product.Id);

        //Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetAvailableProductResults_Returns_Available_Product()
    {
        await using var context = DbContextFactory.CreateInstance("GetAvailableProductResults_Returns_Available_Product");
            
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        var article2 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article2);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("GetAvailableProductResults_Returns_Available_Product");
        var productService = new ProductService(serviceContext);

        //Act
        var result = await productService.Find();

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetAvailableProductResults_Returns_Available_Product_WithOrderLine()
    {
        await using var context = DbContextFactory.CreateInstance("GetAvailableProductResults_Returns_Available_Product_WithOrderLine");
            
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        var article2 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article2);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);
        context.OrderLines.Add(orderLine);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("GetAvailableProductResults_Returns_Available_Product_WithOrderLine");
        var productService = new ProductService(serviceContext);
        //Act
        var result = await productService.Find();

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetAvailableProductResults_Returns_No_Available_Product_When_All_Rented()
    {
        await using var context = DbContextFactory.CreateInstance("GetAvailableProductResults_Returns_No_Available_Product_When_All_Rented");
            
        //Arrange
        var customer = CustomerFactory.CreateValidEntity();
        context.Customers.Add(customer);
        var product = ProductFactory.CreateValidEntity();
        context.Products.Add(product);
        var article = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article);
        var article2 = ArticleFactory.CreateValidEntity(product);
        context.Articles.Add(article2);
        var order = OrderFactory.CreateValidEntity(customer);
        context.Orders.Add(order);
        var orderLine = OrderLineFactory.CreateValidEntity(order, article);
        orderLine.RentedAt = new DateTime(2022, 1, 1);
        context.OrderLines.Add(orderLine);
        var orderLine2 = OrderLineFactory.CreateValidEntity(order, article2);
        orderLine2.RentedAt = new DateTime(2022, 1, 5);
        context.OrderLines.Add(orderLine2);
        await context.SaveChangesAsync();

        await using var serviceContext = DbContextFactory.CreateInstance("GetAvailableProductResults_Returns_No_Available_Product_When_All_Rented");
        var productService = new ProductService(serviceContext);

        //Act
        var filter = new ProductFilter
        {
            AvailableFromDateTime = new DateTime(2021, 12, 1),
            AvailableUntilDateTime = new DateTime(2022, 1, 10)
        };
        var result = await productService.Find(filter);

        Assert.AreEqual(0, result.Count);
    }
}