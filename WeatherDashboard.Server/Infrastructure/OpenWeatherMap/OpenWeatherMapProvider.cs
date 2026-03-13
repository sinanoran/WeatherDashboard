using System.Text.Json;
using WeatherDashboard.Server.Domain.Interfaces;
using WeatherDashboard.Server.Domain.Models;
using WeatherDashboard.Server.Infrastructure.OpenWeatherMap.Dtos;

namespace WeatherDashboard.Server.Infrastructure.OpenWeatherMap;

/// <summary>
/// Fetches live weather data from the OpenWeatherMap Weather API 2.5.
/// Uses the Geocoding API to resolve city names to coordinates.
/// Falls back to deterministic dummy data when no API key is configured.
/// </summary>
public sealed class OpenWeatherMapProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<OpenWeatherMapProvider> logger) : IWeatherProvider
{
    public async Task<WeatherInfo?> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        string apiKey = configuration["OpenWeatherMap:ApiKey"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("OpenWeatherMap API key not configured. Returning fallback data.");
            return GetFallbackWeather(city);
        }

        try
        {
            OpenWeatherMapGeocodingResult? geocodingResult = await GeocodeAsync(city, apiKey, cancellationToken);

            if (geocodingResult is null)
            {
                logger.LogWarning("City not found: {City}", city);
                return null;
            }

            OpenWeatherMapWeatherResponse? weatherResponse = await FetchWeatherAsync(
                geocodingResult.Lat,
                geocodingResult.Lon,
                apiKey,
                cancellationToken);

            if (weatherResponse is null)
            {
                return null;
            }

            return MapToDomain(geocodingResult.Name, weatherResponse);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error fetching weather for {City}", city);
            return GetFallbackWeather(city);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Failed to deserialize weather response for {City}", city);
            return GetFallbackWeather(city);
        }
    }

    private async Task<OpenWeatherMapGeocodingResult?> GeocodeAsync(
        string city, string apiKey, CancellationToken cancellationToken)
    {
        List<OpenWeatherMapGeocodingResult>? geocodingResults = await httpClient.GetFromJsonAsync<List<OpenWeatherMapGeocodingResult>>(
            $"geo/1.0/direct?q={Uri.EscapeDataString(city)}&limit=1&appid={apiKey}",
            cancellationToken);

        if (geocodingResults is null || geocodingResults.Count == 0)
        {
            return null;
        }

        return geocodingResults[0];
    }

    private async Task<OpenWeatherMapWeatherResponse?> FetchWeatherAsync(
        double latitude, double longitude, string apiKey, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await httpClient.GetAsync(
            $"data/2.5/weather?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<OpenWeatherMapWeatherResponse>(cancellationToken);
    }

    private static WeatherInfo MapToDomain(string cityName, OpenWeatherMapWeatherResponse weatherResponse)
    {
        OpenWeatherMapWeather? firstWeather = weatherResponse.Weather.FirstOrDefault();

        return new WeatherInfo(
            City: cityName,
            TemperatureCelsius: weatherResponse.Main.Temp,
            Humidity: weatherResponse.Main.Humidity,
            WindSpeedMps: weatherResponse.Wind.Speed,
            Description: firstWeather?.Description ?? "N/A",
            IconCode: firstWeather?.Icon ?? string.Empty);
    }

    private static WeatherInfo GetFallbackWeather(string city)
    {
        int hash = Math.Abs(city.GetHashCode(StringComparison.OrdinalIgnoreCase));

        return new WeatherInfo(
            City: city,
            TemperatureCelsius: hash % 45 - 10,
            Humidity: 30 + hash % 60,
            WindSpeedMps: 1.0 + hash % 15,
            Description: "scattered clouds",
            IconCode: "03d");
    }
}
