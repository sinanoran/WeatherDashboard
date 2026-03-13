using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using WeatherDashboard.Server.Domain.Models;
using WeatherDashboard.Server.Infrastructure.OpenWeatherMap;
using WeatherDashboard.Server.Infrastructure.OpenWeatherMap.Dtos;
using Xunit;

namespace WeatherDashboard.Server.Tests;

public class OpenWeatherMapProviderTests : IDisposable
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configWithKey;
    private readonly IConfiguration _configWithoutKey;

    public OpenWeatherMapProviderTests()
    {
        _httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri("https://api.openweathermap.org/")
        };

        _configWithKey = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenWeatherMap:ApiKey"] = "test-key"
            })
            .Build();

        _configWithoutKey = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenWeatherMap:ApiKey"] = ""
            })
            .Build();
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsFallback_WhenNoApiKeyConfigured()
    {
        OpenWeatherMapProvider sut = CreateProvider(_configWithoutKey);

        WeatherInfo? result = await sut.GetWeatherAsync("London");

        Assert.NotNull(result);
        Assert.Equal("London", result.City);
        Assert.Equal("scattered clouds", result.Description);
        Assert.Equal("03d", result.IconCode);
    }

    [Fact]
    public async Task GetWeatherAsync_DoesNotCallApi_WhenNoApiKeyConfigured()
    {
        OpenWeatherMapProvider sut = CreateProvider(_configWithoutKey);

        await sut.GetWeatherAsync("London");

        Assert.Equal(0, _handler.CallCount);
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsNull_WhenGeocodingReturnsEmptyArray()
    {
        _handler.SetupResponses(
            CreateJsonResponse(HttpStatusCode.OK, new List<OpenWeatherMapGeocodingResult>()));

        OpenWeatherMapProvider sut = CreateProvider(_configWithKey);

        WeatherInfo? result = await sut.GetWeatherAsync("InvalidCity12345");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsMappedDomainObject_WhenApiReturnsSuccess()
    {
        List<OpenWeatherMapGeocodingResult> geocodingResults =
        [
            new OpenWeatherMapGeocodingResult { Name = "London", Lat = 51.5074, Lon = -0.1278, Country = "GB" }
        ];

        OpenWeatherMapWeatherResponse weatherResponse = new OpenWeatherMapWeatherResponse
        {
            Name = "London",
            Main = new OpenWeatherMapMainData { Temp = 15.5, Humidity = 72 },
            Wind = new OpenWeatherMapWindData { Speed = 3.6 },
            Weather = [new OpenWeatherMapWeather { Description = "light rain", Icon = "10d" }]
        };

        _handler.SetupResponses(
            CreateJsonResponse(HttpStatusCode.OK, geocodingResults),
            CreateJsonResponse(HttpStatusCode.OK, weatherResponse));

        OpenWeatherMapProvider sut = CreateProvider(_configWithKey);

        WeatherInfo? result = await sut.GetWeatherAsync("London");

        Assert.NotNull(result);
        Assert.Equal("London", result.City);
        Assert.Equal(15.5, result.TemperatureCelsius);
        Assert.Equal(72, result.Humidity);
        Assert.Equal(3.6, result.WindSpeedMps);
        Assert.Equal("light rain", result.Description);
        Assert.Equal("10d", result.IconCode);
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsFallback_WhenHttpRequestFails()
    {
        _handler.ExceptionToThrow = new HttpRequestException("Connection refused");
        OpenWeatherMapProvider sut = CreateProvider(_configWithKey);

        WeatherInfo? result = await sut.GetWeatherAsync("London");

        Assert.NotNull(result);
        Assert.Equal("London", result.City);
        Assert.Equal("scattered clouds", result.Description);
    }

    [Fact]
    public async Task GetWeatherAsync_HandlesEmptyWeatherList()
    {
        List<OpenWeatherMapGeocodingResult> geocodingResults =
        [
            new OpenWeatherMapGeocodingResult { Name = "TestCity", Lat = 40.0, Lon = -74.0, Country = "US" }
        ];

        OpenWeatherMapWeatherResponse weatherResponse = new OpenWeatherMapWeatherResponse
        {
            Name = "TestCity",
            Main = new OpenWeatherMapMainData { Temp = 20, Humidity = 50 },
            Wind = new OpenWeatherMapWindData { Speed = 1.0 },
            Weather = []
        };

        _handler.SetupResponses(
            CreateJsonResponse(HttpStatusCode.OK, geocodingResults),
            CreateJsonResponse(HttpStatusCode.OK, weatherResponse));

        OpenWeatherMapProvider sut = CreateProvider(_configWithKey);

        WeatherInfo? result = await sut.GetWeatherAsync("TestCity");

        Assert.NotNull(result);
        Assert.Equal("N/A", result.Description);
        Assert.Equal(string.Empty, result.IconCode);
    }

    [Fact]
    public async Task GetWeatherAsync_FallbackIsDeterministic_ForSameCity()
    {
        OpenWeatherMapProvider sut = CreateProvider(_configWithoutKey);

        WeatherInfo? result1 = await sut.GetWeatherAsync("Paris");
        WeatherInfo? result2 = await sut.GetWeatherAsync("Paris");

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.TemperatureCelsius, result2.TemperatureCelsius);
        Assert.Equal(result1.Humidity, result2.Humidity);
        Assert.Equal(result1.WindSpeedMps, result2.WindSpeedMps);
    }

    [Fact]
    public async Task GetWeatherAsync_SendsGeocodingRequestFirst()
    {
        _handler.SetupResponses(
            CreateJsonResponse(HttpStatusCode.OK, new List<OpenWeatherMapGeocodingResult>()));

        OpenWeatherMapProvider sut = CreateProvider(_configWithKey);

        await sut.GetWeatherAsync("New York");

        Assert.Equal(1, _handler.CallCount);
        Assert.NotNull(_handler.RequestUris[0]);
        string geocodingUrl = _handler.RequestUris[0]!.AbsoluteUri;
        Assert.Contains("geo/1.0/direct", geocodingUrl);
        Assert.Contains("q=New%20York", geocodingUrl);
        Assert.Contains("appid=test-key", geocodingUrl);
    }

    [Fact]
    public async Task GetWeatherAsync_SendsWeatherRequestWithCoordinates()
    {
        List<OpenWeatherMapGeocodingResult> geocodingResults =
        [
            new OpenWeatherMapGeocodingResult { Name = "Berlin", Lat = 52.52, Lon = 13.405, Country = "DE" }
        ];

        OpenWeatherMapWeatherResponse weatherResponse = new OpenWeatherMapWeatherResponse
        {
            Name = "Berlin",
            Main = new OpenWeatherMapMainData { Temp = 10, Humidity = 60 },
            Wind = new OpenWeatherMapWindData { Speed = 2.0 },
            Weather = [new OpenWeatherMapWeather { Description = "cloudy", Icon = "04d" }]
        };

        _handler.SetupResponses(
            CreateJsonResponse(HttpStatusCode.OK, geocodingResults),
            CreateJsonResponse(HttpStatusCode.OK, weatherResponse));

        OpenWeatherMapProvider sut = CreateProvider(_configWithKey);

        await sut.GetWeatherAsync("Berlin");

        Assert.Equal(2, _handler.CallCount);
        Assert.NotNull(_handler.RequestUris[1]);
        string weatherUrl = _handler.RequestUris[1]!.AbsoluteUri;
        Assert.Contains("data/2.5/weather", weatherUrl);
        Assert.Contains("lat=52.52", weatherUrl);
        Assert.Contains("lon=13.405", weatherUrl);
        Assert.Contains("units=metric", weatherUrl);
    }

    [Fact]
    public async Task GetWeatherAsync_UsesCityNameFromGeocoding()
    {
        List<OpenWeatherMapGeocodingResult> geocodingResults =
        [
            new OpenWeatherMapGeocodingResult { Name = "Praha", Lat = 50.08, Lon = 14.42, Country = "CZ" }
        ];

        OpenWeatherMapWeatherResponse weatherResponse = new OpenWeatherMapWeatherResponse
        {
            Name = "Prague",
            Main = new OpenWeatherMapMainData { Temp = 12, Humidity = 65 },
            Wind = new OpenWeatherMapWindData { Speed = 3.0 },
            Weather = [new OpenWeatherMapWeather { Description = "clear sky", Icon = "01d" }]
        };

        _handler.SetupResponses(
            CreateJsonResponse(HttpStatusCode.OK, geocodingResults),
            CreateJsonResponse(HttpStatusCode.OK, weatherResponse));

        OpenWeatherMapProvider sut = CreateProvider(_configWithKey);

        WeatherInfo? result = await sut.GetWeatherAsync("Prague");

        Assert.NotNull(result);
        Assert.Equal("Praha", result.City);
    }

    private static HttpResponseMessage CreateJsonResponse<T>(HttpStatusCode statusCode, T body)
    {
        string json = JsonSerializer.Serialize(body);
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }

    private OpenWeatherMapProvider CreateProvider(IConfiguration configuration) =>
        new(_httpClient, configuration, NullLogger<OpenWeatherMapProvider>.Instance);

    public void Dispose() => _httpClient.Dispose();
}

/// <summary>
/// Mock HttpMessageHandler that supports sequenced responses for multi-step API calls.
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responseQueue = new();

    public HttpRequestException? ExceptionToThrow { get; set; }
    public int CallCount { get; private set; }
    public List<Uri?> RequestUris { get; } = [];

    public void SetupResponses(params HttpResponseMessage[] responses)
    {
        foreach (HttpResponseMessage response in responses)
        {
            _responseQueue.Enqueue(response);
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        RequestUris.Add(request.RequestUri);

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        if (_responseQueue.Count > 0)
        {
            return Task.FromResult(_responseQueue.Dequeue());
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
