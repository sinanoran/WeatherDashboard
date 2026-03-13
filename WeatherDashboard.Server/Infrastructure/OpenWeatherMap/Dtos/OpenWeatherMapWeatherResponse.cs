using System.Text.Json.Serialization;

namespace WeatherDashboard.Server.Infrastructure.OpenWeatherMap.Dtos;

/// <summary>
/// Root response from the OpenWeatherMap Weather API 2.5.
/// GET data/2.5/weather?lat={lat}&amp;lon={lon}&amp;appid={key}&amp;units=metric
/// </summary>
public sealed class OpenWeatherMapWeatherResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("main")]
    public OpenWeatherMapMainData Main { get; set; } = new();

    [JsonPropertyName("wind")]
    public OpenWeatherMapWindData Wind { get; set; } = new();

    [JsonPropertyName("weather")]
    public List<OpenWeatherMapWeather> Weather { get; set; } = [];
}
