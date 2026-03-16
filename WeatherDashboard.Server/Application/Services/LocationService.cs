using WeatherDashboard.Server.Application.Interfaces;
using WeatherDashboard.Server.Domain.Interfaces;

namespace WeatherDashboard.Server.Application.Services;

public sealed class LocationService(
    ILocationRepository locationRepository,
    ILogger<LocationService> logger) : ILocationService
{
    private const string FallbackCity = "London";

    public async Task<string> GetDefaultLocationAsync(CancellationToken cancellationToken = default)
    {
        string? city = await locationRepository.GetDefaultCityAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(city))
        {
            logger.LogInformation("No default location set. Using fallback: {City}", FallbackCity);
            return FallbackCity;
        }

        return city;
    }

    public async Task SetDefaultLocationAsync(string city, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Setting default location to {City}", city);
        await locationRepository.SetDefaultCityAsync(city.Trim(), cancellationToken);
    }
}
