namespace WeatherDashboard.Server.Application.Interfaces;

public interface ILocationService
{
    Task<string> GetDefaultLocationAsync(CancellationToken cancellationToken = default);
    Task SetDefaultLocationAsync(string city, CancellationToken cancellationToken = default);
}
