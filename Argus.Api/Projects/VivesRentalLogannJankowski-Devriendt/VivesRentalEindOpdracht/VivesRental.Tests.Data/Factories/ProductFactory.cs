using VivesRental.Model;

namespace VivesRental.Tests.Data.Factories;

public static class ProductFactory
{
    public static Product CreateValidEntity()
    {
        return new Product
        {
            Name = "TestName",
            Description = "TestDescription",
            Manufacturer = "TestManufacturer",
            Publisher = "TestPublisher",
            RentalExpiresAfterDays = 10
        };
    }

    public static Product CreateInvalidEntity()
    {
        return new Product
        {
            Description = "TestDescription",
            Manufacturer = "TestManufacturer",
            Publisher = "TestPublisher"
        };
    }
}