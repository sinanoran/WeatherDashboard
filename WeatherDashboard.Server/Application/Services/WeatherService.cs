using WeatherDashboard.Server.Application.Contracts;
using WeatherDashboard.Server.Application.Interfaces;
using WeatherDashboard.Server.Domain.Interfaces;
using WeatherDashboard.Server.Domain.Models;

namespace WeatherDashboard.Server.Application.Services;

public sealed class WeatherService(
    IWeatherProvider weatherProvider,
    ILogger<WeatherService> logger) : IWeatherService
{
    public async Task<WeatherResponse?> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Requesting weather for {City}", city);

        WeatherInfo? info = await weatherProvider.GetWeatherAsync(city, cancellationToken);

        return info is not null ? WeatherResponse.FromWeatherInfo(info) : null;
    }
}
