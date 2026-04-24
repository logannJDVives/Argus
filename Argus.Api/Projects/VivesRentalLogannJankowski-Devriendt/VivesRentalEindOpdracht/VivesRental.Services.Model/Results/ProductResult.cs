namespace VivesRental.Services.Model.Results;

public class ProductResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? Publisher { get; set; }
    public int RentalExpiresAfterDays { get; set; }
    public int NumberOfArticles { get; set; }
    public int NumberOfAvailableArticles { get; set; }
}