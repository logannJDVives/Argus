namespace VivesRental.Services.Model.Filters;

public class ArticleReservationFilter
{
    public Guid? ArticleId { get; set; }
    public Guid? CustomerId { get; set; }
}