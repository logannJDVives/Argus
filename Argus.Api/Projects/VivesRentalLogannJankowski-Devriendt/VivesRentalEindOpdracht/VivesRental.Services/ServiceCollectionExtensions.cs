using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VivesRental.Repository.Core;
using VivesRental.Services.Abstractions;

namespace VivesRental.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVivesRentalServices(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<VivesRentalDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderLineService, OrderLineService>();
        services.AddScoped<IArticleReservationService, ArticleReservationService>();

        return services;
    }
}
