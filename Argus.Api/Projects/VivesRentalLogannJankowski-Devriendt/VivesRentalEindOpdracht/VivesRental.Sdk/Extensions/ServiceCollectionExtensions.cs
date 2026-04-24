using Microsoft.Extensions.DependencyInjection;
using VivesRental.Sdk.Handlers;
using VivesRental.Sdk.Interfaces;
using VivesRental.Sdk.Services;
using VivesRental.Sdk.Stores;

namespace VivesRental.Sdk.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVivesRentalSdk(this IServiceCollection services, string apiBaseUrl)
    {
        // Register TokenStore as singleton (behoud token tijdens sessie)
        services.AddSingleton<ITokenStore, TokenStore>();

        // Register AuthorizationHandler
        services.AddTransient<AuthorizationHandler>();

        // Register Auth SDK (zonder handler, want login vereist geen token)
        services.AddHttpClient<IAuthSdk, AuthSdk>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        // Register alle andere SDKs met AuthorizationHandler
        services.AddHttpClient<IProductSdk, ProductSdk>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();

        services.AddHttpClient<ICustomerSdk, CustomerSdk>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();

        services.AddHttpClient<IArticleSdk, ArticleSdk>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();

        services.AddHttpClient<IOrderSdk, OrderSdk>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();

        services.AddHttpClient<IOrderLineSdk, OrderLineSdk>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();

        services.AddHttpClient<IArticleReservationSdk, ArticleReservationSdk>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<AuthorizationHandler>();

        return services;
    }
}
