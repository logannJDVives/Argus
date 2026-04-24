namespace VivesRental.Dto.Requests;

public class OrderLineRequest
{
    public Guid OrderId { get; set; }
    public Guid ArticleId { get; set; }
}
