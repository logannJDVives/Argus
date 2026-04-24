using VivesRental.Model;
using VivesRental.Services.Model.Filters;
using VivesRental.Services.Model.Results;

namespace VivesRental.Services.Mappers;

/// <summary>
/// https://stackoverflow.com/questions/39585427/projection-of-single-entities-in-ef-with-extension-methods
/// </summary>
public static class EntityMappingExtensions
{
    public static ArticleResult MapToResult(this Article article)
    {
        return ProjectionExpressions.ProjectToArticleResult().Compile()(article);
    }

    public static ArticleReservationResult MapToResult(this ArticleReservation articleReservation)
    {
        return ProjectionExpressions.ProjectToArticleReservationResult().Compile()(articleReservation);
    }

    public static CustomerResult MapToResult(this Customer customer)
    {
        return ProjectionExpressions.ProjectToCustomerResult().Compile()(customer);
    }

    public static OrderResult MapToResult(this Order order)
    {
        return ProjectionExpressions.ProjectToOrderResult().Compile()(order);
    }

    public static OrderLineResult MapToResult(this OrderLine orderLine)
    {
        return ProjectionExpressions.ProjectToOrderLineResult().Compile()(orderLine);
    }

    public static ProductResult MapToResult(this Product product, ProductFilter? filter = null)
    {
        return ProjectionExpressions.ProjectToProductResult(filter).Compile()(product);
    }
}