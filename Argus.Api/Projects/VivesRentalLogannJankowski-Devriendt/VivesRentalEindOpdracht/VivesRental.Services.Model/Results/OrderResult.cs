namespace VivesRental.Services.Model.Results;

public class OrderResult
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhoneNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int NumberOfOrderLines { get; set; }
}