using System.Text.Json.Serialization;

namespace WeatherDashboard.Server.Infrastructure.OpenWeatherMap.Dtos;

/// <summary>
/// Represents a single result from the OpenWeatherMap Geocoding API.
/// GET geo/1.0/direct?q={city}&amp;limit=1&amp;appid={key}
/// </summary>
public sealed class OpenWeatherMapGeocodingResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}
