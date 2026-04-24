namespace VivesRental.Services.Model.Filters;

public class ProductFilter
{
    public DateTime? AvailableFromDateTime { get; set; }
    public DateTime? AvailableUntilDateTime { get; set; }
}