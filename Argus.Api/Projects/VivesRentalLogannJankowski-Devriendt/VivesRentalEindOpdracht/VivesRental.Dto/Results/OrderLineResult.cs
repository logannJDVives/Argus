namespace VivesRental.Dto.Results;

public class OrderLineResult
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ArticleId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductDescription { get; set; }
    public DateTime RentedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
}
