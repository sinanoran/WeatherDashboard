namespace WeatherDashboard.Server.Api.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        RouteGroupBuilder api = app.MapGroup("/api");
        api.MapWeatherEndpoints();
        api.MapLocationEndpoints();

        return app;
    }
}
