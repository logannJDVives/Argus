namespace VivesRental.Services.Model.Filters;

public class OrderFilter
{
    public Guid? CustomerId { get; set; }
    public string? Search { get; set; }
}