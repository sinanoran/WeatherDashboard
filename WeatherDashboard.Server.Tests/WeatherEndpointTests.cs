using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WeatherDashboard.Server.Application.Contracts;
using WeatherDashboard.Server.Application.Interfaces;
using Xunit;

namespace WeatherDashboard.Server.Tests;

public class WeatherEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Mock<IWeatherService> weatherService = new Mock<IWeatherService>();
                weatherService
                    .Setup(service => service.GetWeatherAsync("London", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new WeatherResponse(
                        "London",
                        15,
                        59,
                        60,
                        3.5,
                        "clear sky",
                        "https://openweathermap.org/img/wn/01d@2x.png"));
                weatherService
                    .Setup(service => service.GetWeatherAsync("InvalidCity12345", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((WeatherResponse?)null);
                weatherService
                    .Setup(service => service.GetWeatherAsync("CrashCity", It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("Simulated failure."));

                Mock<ILocationService> locationService = new Mock<ILocationService>();
                locationService
                    .Setup(service => service.GetDefaultLocationAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync("London");

                services.AddSingleton(weatherService.Object);
                services.AddSingleton(locationService.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetWeather_ReturnsWeatherForCity()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/weather?city=London");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        WeatherResponse? weather = await response.Content.ReadFromJsonAsync<WeatherResponse>();
        Assert.NotNull(weather);
        Assert.Equal("London", weather.City);
        Assert.Equal(15, weather.TemperatureC);
    }

    [Fact]
    public async Task GetWeather_Returns404ForUnknownCity()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/weather?city=InvalidCity12345");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetWeather_UsesDefaultLocation_WhenNoCityProvided()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/weather");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        WeatherResponse? weather = await response.Content.ReadFromJsonAsync<WeatherResponse>();
        Assert.NotNull(weather);
        Assert.Equal("London", weather.City);
    }

    [Fact]
    public async Task GetDefaultLocation_ReturnsCity()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/location/default");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LocationDto? body = await response.Content.ReadFromJsonAsync<LocationDto>();
        Assert.Equal("London", body?.City);
    }

    [Fact]
    public async Task SetDefaultLocation_ReturnsOk_WhenCityIsValid()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/location/default", new { city = "London" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LocationDto? body = await response.Content.ReadFromJsonAsync<LocationDto>();
        Assert.Equal("London", body?.City);
    }

    [Fact]
    public async Task SetDefaultLocation_Returns404_WhenCityIsNotFound()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/location/default", new { city = "InvalidCity12345" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetDefaultLocation_Returns400_WhenCityIsEmpty()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/location/default", new { city = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetDefaultLocation_Returns400_WhenCityIsWhitespace()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/location/default", new { city = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWeather_ReturnsProblemDetails_WhenUnhandledExceptionOccurs()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/weather?city=CrashCity");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.InternalServerError, problemDetails.Status);
        Assert.Equal("An unexpected error occurred", problemDetails.Title);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }

    private sealed record LocationDto(string City);
}
