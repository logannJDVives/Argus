using VivesRental.Enums;

namespace VivesRental.Services.Model.Results;

public class ArticleReservationResult
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public ArticleStatus ArticleStatus { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;

    public DateTime FromDateTime { get; set; }
    public DateTime UntilDateTime { get; set; }
    
    // Status velden
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? RejectionReason { get; set; }
}