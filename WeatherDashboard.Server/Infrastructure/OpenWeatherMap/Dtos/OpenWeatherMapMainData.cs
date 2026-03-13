using System.Text.Json.Serialization;

namespace WeatherDashboard.Server.Infrastructure.OpenWeatherMap.Dtos;

/// <summary>
/// The "main" object within the Weather API 2.5 response.
/// </summary>
public sealed class OpenWeatherMapMainData
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}
