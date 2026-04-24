using System.ComponentModel.DataAnnotations;

namespace VivesRental.Services.Model.Requests;

public class ProductRequest
{
    [Required] public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? Publisher { get; set; }
    public int RentalExpiresAfterDays { get; set; }
}