using VivesRental.Model;

namespace VivesRental.Tests.Data.Factories;

public static class OrderFactory
{
    public static Order CreateValidEntity(Customer customer)
    {
        var order = new Order
        {
            CustomerId = customer.Id,
            Customer = customer,
            CustomerFirstName = "TestFirstName",
            CustomerLastName = "TestLastName",
            CustomerEmail = "TestEmail",
            CustomerPhoneNumber = "TestPhoneNumber",
            CreatedAt = DateTime.Now
        };
        customer.Orders.Add(order);
        return order;
    }

    public static Order CreateInvalidEntity()
    {
        return new Order
        {
            CreatedAt = DateTime.Now
        };
    }
}