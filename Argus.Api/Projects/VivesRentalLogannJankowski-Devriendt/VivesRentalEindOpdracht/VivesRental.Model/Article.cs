using VivesRental.Enums;

namespace VivesRental.Model;

public class Article
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public ArticleStatus Status { get; set; }

    public IList<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    public IList<ArticleReservation> ArticleReservations { get; set; } = new List<ArticleReservation>();
}