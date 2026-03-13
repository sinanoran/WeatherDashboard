using WeatherDashboard.Server.Domain.Models;

namespace WeatherDashboard.Server.Domain.Interfaces;

/// <summary>
/// Domain port for retrieving weather data from an external source.
/// </summary>
public interface IWeatherProvider
{
    Task<WeatherInfo?> GetWeatherAsync(string city, CancellationToken cancellationToken = default);
}
