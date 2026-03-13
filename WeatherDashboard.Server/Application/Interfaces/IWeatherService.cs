using WeatherDashboard.Server.Application.Contracts;

namespace WeatherDashboard.Server.Application.Interfaces;

public interface IWeatherService
{
    Task<WeatherResponse?> GetWeatherAsync(string city, CancellationToken cancellationToken = default);
}
