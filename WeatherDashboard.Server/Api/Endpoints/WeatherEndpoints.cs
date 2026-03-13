using Microsoft.AspNetCore.Http.HttpResults;
using WeatherDashboard.Server.Application.Contracts;
using WeatherDashboard.Server.Application.Interfaces;

namespace WeatherDashboard.Server.Api.Endpoints;

public static class WeatherEndpoints
{
    public static RouteGroupBuilder MapWeatherEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("weather", async (
            string? city,
            IWeatherService weatherService,
            ILocationService locationService,
            CancellationToken cancellationToken) =>
        {
            string resolvedCity = string.IsNullOrWhiteSpace(city)
                ? await locationService.GetDefaultLocationAsync(cancellationToken)
                : city.Trim();

            if (resolvedCity.Length > 100)
            {
                return Results.BadRequest(new { error = "City name is too long." });
            }

            WeatherResponse? result = await weatherService.GetWeatherAsync(resolvedCity, cancellationToken);

            return result is not null
                ? Results.Ok(result)
                : Results.NotFound(new { error = $"City '{resolvedCity}' not found." });
        })
        .WithName("GetWeather")
        .WithTags("Weather")
        .WithSummary("Get current weather")
        .WithDescription("Retrieves the current weather for a specified city. If no city is provided, it returns the weather for the default location.")
        .Produces<WeatherResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

		return group;
    }
}
