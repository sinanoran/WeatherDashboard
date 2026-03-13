namespace WeatherDashboard.Server.Domain.Interfaces;

/// <summary>
/// Domain port for persisting and retrieving the user's default location.
/// </summary>
public interface ILocationRepository
{
    Task<string?> GetDefaultCityAsync(CancellationToken cancellationToken = default);
    Task SetDefaultCityAsync(string city, CancellationToken cancellationToken = default);
}
