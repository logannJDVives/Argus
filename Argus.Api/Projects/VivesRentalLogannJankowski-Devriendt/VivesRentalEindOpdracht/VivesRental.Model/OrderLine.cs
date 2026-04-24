namespace VivesRental.Model;

public class OrderLine
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? ArticleId { get; set; }
    public Article Article { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? ProductDescription { get; set; }
    public DateTime RentedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
}