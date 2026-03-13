namespace WeatherDashboard.Server.Domain.Models;

/// <summary>
/// Core value object representing weather data for a city.
/// Contains only domain-relevant data - no presentation or infrastructure concerns.
/// </summary>
public sealed record WeatherInfo(
    string City,
    double TemperatureCelsius,
    int Humidity,
    double WindSpeedMps,
    string Description,
    string IconCode);
