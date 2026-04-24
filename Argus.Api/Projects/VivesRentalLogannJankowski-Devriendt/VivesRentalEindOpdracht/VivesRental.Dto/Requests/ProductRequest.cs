namespace VivesRental.Dto.Requests;

public class ProductRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? Publisher { get; set; }
    public int RentalExpiresAfterDays { get; set; }
}
