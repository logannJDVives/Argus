namespace VivesRental.Services.Model.Requests;

public class ArticleReservationRequest
{
    public Guid ArticleId { get; set; }
    public Guid CustomerId { get; set; }

    public DateTime? FromDateTime { get; set; }
    public DateTime? UntilDateTime { get; set; }
}