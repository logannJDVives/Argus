using VivesRental.Enums;

namespace VivesRental.Services.Model.Requests;

public class ArticleRequest
{
    public Guid ProductId { get; set; }
    public ArticleStatus Status { get; set; }
}