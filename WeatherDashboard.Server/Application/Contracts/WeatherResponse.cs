using WeatherDashboard.Server.Domain.Models;

namespace WeatherDashboard.Server.Application.Contracts;

public sealed record WeatherResponse(
    string City,
    double TemperatureC,
    double TemperatureF,
    int Humidity,
    double WindSpeedMps,
    string Description,
    string Icon)
{
    public static WeatherResponse FromWeatherInfo(WeatherInfo info)
    {
        double temperatureC = Math.Round(info.TemperatureCelsius, 1);

        return new WeatherResponse(
            City: info.City,
            TemperatureC: temperatureC,
            TemperatureF: (temperatureC * 9.0 / 5.0) + 32.0,
            Humidity: info.Humidity,
            WindSpeedMps: Math.Round(info.WindSpeedMps, 1),
            Description: info.Description,
            Icon: string.IsNullOrEmpty(info.IconCode)
                ? string.Empty
                : $"https://openweathermap.org/img/wn/{info.IconCode}@2x.png");
    }
}
