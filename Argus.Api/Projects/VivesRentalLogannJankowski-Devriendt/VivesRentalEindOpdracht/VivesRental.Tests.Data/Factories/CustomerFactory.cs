using VivesRental.Model;

namespace VivesRental.Tests.Data.Factories;

public static class CustomerFactory
{
    public static Customer CreateValidEntity()
    {
        return new Customer
        {
            FirstName = "TestFirstName",
            LastName = "TestName",
            Email = "TestEmail",
            PhoneNumber = "TestPhoneNumber"
        };
    }

    public static Customer CreateInvalidEntity()
    {
        return new Customer
        {
            PhoneNumber = "TestPhoneNumber"
        };
    }
}