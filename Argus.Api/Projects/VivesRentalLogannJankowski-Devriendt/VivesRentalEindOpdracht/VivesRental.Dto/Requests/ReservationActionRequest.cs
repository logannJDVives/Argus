namespace VivesRental.Dto.Requests;

public class ApproveReservationRequest
{
    public Guid ReservationId { get; set; }
}

public class RejectReservationRequest
{
    public Guid ReservationId { get; set; }
    public string? Reason { get; set; }
}
