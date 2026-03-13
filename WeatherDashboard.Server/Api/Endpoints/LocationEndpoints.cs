using WeatherDashboard.Server.Application.Contracts;
using WeatherDashboard.Server.Application.Interfaces;

namespace WeatherDashboard.Server.Api.Endpoints;

public static class LocationEndpoints
{
    public static RouteGroupBuilder MapLocationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("location/default", async (ILocationService locationService, CancellationToken cancellationToken) =>
        {
            string city = await locationService.GetDefaultLocationAsync(cancellationToken);
            return Results.Ok(new DefaultLocationResponse(city));
        })
        .WithName("GetDefaultLocation")
        .WithTags("Location")
        .WithSummary("Get default location")
        .WithDescription("Retrieves the default location for weather forecasts. Returns the city name as a string.")
        .Produces<DefaultLocationResponse>(StatusCodes.Status200OK);

		group.MapPut("location/default", async (
            DefaultLocationRequest request,
            ILocationService locationService,
            IWeatherService weatherService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.City))
            {
                return Results.BadRequest(new { error = "City name is required." });
            }

            if (request.City.Trim().Length > 100)
            {
                return Results.BadRequest(new { error = "City name is too long." });
            }

            WeatherResponse? weather = await weatherService.GetWeatherAsync(request.City, cancellationToken);

            if (weather is null)
            {
                return Results.NotFound(new { error = $"City '{request.City}' not found." });
            }

            await locationService.SetDefaultLocationAsync(weather.City, cancellationToken);

            return Results.Ok(new DefaultLocationResponse(weather.City));
        })
        .WithName("SetDefaultLocation")
        .WithTags("Location")
        .WithSummary("Set default location")
        .WithDescription("Sets the default location for weather forecasts. The city name must be valid and exist in the weather service.")
        .Produces<DefaultLocationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

		return group;
    }
}
