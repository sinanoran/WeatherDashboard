using System.Text.Json.Serialization;

namespace WeatherDashboard.Server.Infrastructure.OpenWeatherMap.Dtos;

/// <summary>
/// The "wind" object within the Weather API 2.5 response.
/// </summary>
public sealed class OpenWeatherMapWindData
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}
