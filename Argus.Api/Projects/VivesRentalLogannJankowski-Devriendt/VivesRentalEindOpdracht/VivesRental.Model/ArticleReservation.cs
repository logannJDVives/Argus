using VivesRental.Enums;

namespace VivesRental.Model;

public class ArticleReservation
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTime FromDateTime { get; set; }
    public DateTime UntilDateTime { get; set; }
    
    // Status voor goedkeuring workflow
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }  // Datum van goedkeuring/afkeuring
    public string? RejectionReason { get; set; } // Reden bij afkeuring
}