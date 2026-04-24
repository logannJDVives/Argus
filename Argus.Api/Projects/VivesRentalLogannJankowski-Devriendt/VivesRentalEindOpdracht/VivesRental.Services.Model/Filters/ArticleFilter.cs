
namespace VivesRental.Services.Model.Filters;

public class ArticleFilter
{
    public IList<Guid>? ArticleIds { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? RentedByCustomerId { get; set; }
    public Guid? ReservedByCustomerId { get; set; }

    public DateTime? AvailableFromDateTime { get; set; }
    public DateTime? AvailableUntilDateTime { get; set; }

    public DateTime? RentedFromDateTime { get; set; }
    public DateTime? RentedUntilDateTime { get; set; }

    public DateTime? ReservedFromDateTime { get; set; }
    public DateTime? ReservedUntilDateTime { get; set; }
}