using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WeatherDashboard.Server.Infrastructure.ErrorHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
	private readonly ILogger<GlobalExceptionHandler> _logger;
	private readonly IProblemDetailsService _problemDetailsService;
	private readonly IHostEnvironment _environment;

	private readonly record struct ExceptionHandlingResult(int StatusCode, string Title, LogLevel LogLevel);

	public GlobalExceptionHandler(
		ILogger<GlobalExceptionHandler> logger,
		IProblemDetailsService problemDetailsService,
		IHostEnvironment environment)
	{
		_logger = logger;
		_problemDetailsService = problemDetailsService;
		_environment = environment;
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		if (httpContext.Response.HasStarted)
		{
			_logger.LogWarning(exception, "Unable to handle exception for {Method} {Path} because the response has already started.", httpContext.Request.Method, httpContext.Request.Path);
			return false;
		}

		ExceptionHandlingResult handlingResult = GetExceptionDetails(exception, httpContext.RequestAborted.IsCancellationRequested);

		_logger.Log(handlingResult.LogLevel, exception, "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}", httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);

		ProblemDetails problemDetails = new()
		{
			Status = handlingResult.StatusCode,
			Title = handlingResult.Title,
			Detail = _environment.IsDevelopment() ? exception.Message : null
		};
		problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

		httpContext.Response.StatusCode = handlingResult.StatusCode;

		bool handled = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = httpContext,
			Exception = exception,
			ProblemDetails = problemDetails
		});

		if (handled)
		{
			return true;
		}

		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
		return true;
	}

	private static ExceptionHandlingResult GetExceptionDetails(Exception exception, bool isCancellationRequested)
	{
		bool isRequestAborted = exception is OperationCanceledException && isCancellationRequested;

		if (isRequestAborted)
		{
			return new ExceptionHandlingResult(StatusCodes.Status499ClientClosedRequest, "Request cancelled", LogLevel.Information);
		}

		if (exception is BadHttpRequestException badHttpRequestException)
		{
			return new ExceptionHandlingResult(badHttpRequestException.StatusCode, "Bad request", LogLevel.Warning);
		}

		if (exception is TimeoutException)
		{
			return new ExceptionHandlingResult(StatusCodes.Status504GatewayTimeout, "The operation timed out", LogLevel.Error);
		}

		return new ExceptionHandlingResult(StatusCodes.Status500InternalServerError, "An unexpected error occurred", LogLevel.Error);
	}
}