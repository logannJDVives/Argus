namespace VivesRental.Model;

public class Order
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; } //Customers can be deleted (GDPR)
    public Customer? Customer { get; set; }

    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhoneNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public IList<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}