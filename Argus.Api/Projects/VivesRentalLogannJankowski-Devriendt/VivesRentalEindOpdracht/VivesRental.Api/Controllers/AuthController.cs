using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VivesRental.Api.Models;
using VivesRental.Api.Services;
using VivesRental.Model;
using VivesRental.Repository.Core;

namespace VivesRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly VivesRentalDbContext _dbContext;

    public AuthController(JwtService jwtService, VivesRentalDbContext dbContext)
    {
        _jwtService = jwtService;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Zoek klant in database met een wachtwoord (account)
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email.ToLower() == request.Email.ToLower() && c.PasswordHash != null);

        if (customer == null)
        {
            return Unauthorized(new { Message = "Ongeldige inloggegevens" });
        }

        // Verifieer wachtwoord
        var passwordHash = HashPassword(request.Password);
        if (customer.PasswordHash != passwordHash)
        {
            return Unauthorized(new { Message = "Ongeldige inloggegevens" });
        }

        // Genereer token
        var token = _jwtService.GenerateToken(customer.Email, customer.Id.ToString(), customer.Role, customer.FirstName, customer.LastName);
        
        return Ok(new LoginResponse 
        { 
            Token = token, 
            Email = customer.Email, 
            Role = customer.Role,
            FirstName = customer.FirstName,
            LastName = customer.LastName
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Controleer of email al bestaat
        var existingCustomer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email.ToLower() == request.Email.ToLower());

        if (existingCustomer != null)
        {
            return BadRequest(new RegisterResponse 
            { 
                Success = false, 
                Message = "Dit e-mailadres is al geregistreerd" 
            });
        }

        // Validatie
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new RegisterResponse 
            { 
                Success = false, 
                Message = "Alle velden zijn verplicht" 
            });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new RegisterResponse 
            { 
                Success = false, 
                Message = "Wachtwoord moet minimaal 6 tekens bevatten" 
            });
        }

        // Maak nieuwe klant aan met account (altijd als "User", nooit als "Admin")
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber ?? "",
            Role = "User", // Altijd User, nooit Admin
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        return Ok(new RegisterResponse 
        { 
            Success = true, 
            Message = "Registratie succesvol! U kunt nu inloggen." 
        });
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
