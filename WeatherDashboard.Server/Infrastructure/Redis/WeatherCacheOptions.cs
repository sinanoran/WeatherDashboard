namespace WeatherDashboard.Server.Infrastructure.Redis;

public sealed class WeatherCacheOptions
{
    public const string SectionPath = "WeatherCache";

    public int ExpirationMinutes { get; init; } = 10;
}
