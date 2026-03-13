using System.Text.Json.Serialization;

namespace WeatherDashboard.Server.Infrastructure.OpenWeatherMap.Dtos;

public sealed class OpenWeatherMapWeather
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}
