namespace VivesRental.Services.Model.Results;

public class CustomerResult
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public int NumberOfOrders { get; set; }
    public int NumberOfPendingOrders { get; set; }
}