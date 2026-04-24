using System.ComponentModel.DataAnnotations;

namespace VivesRental.Services.Model.Requests;

public class CustomerRequest
{
    [Required] public string FirstName { get; set; } = null!;
    [Required] public string LastName { get; set; } = null!;
    [Required] public string Email { get; set; } = null!;
    [Required] public string PhoneNumber { get; set; } = null!;
}