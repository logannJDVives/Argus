using VivesRental.Enums;

namespace VivesRental.Dto.Results;

public class ArticleResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public ArticleStatus Status { get; set; }
}
