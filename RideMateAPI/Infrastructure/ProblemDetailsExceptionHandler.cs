using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RideMateAPI.Infrastructure
{
	public class ProblemDetailsExceptionHandler : IExceptionHandler
	{
		private readonly ILogger<ProblemDetailsExceptionHandler> _logger;

		public ProblemDetailsExceptionHandler(ILogger<ProblemDetailsExceptionHandler> logger)
		{
			_logger = logger;
		}

		public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
		{
			var statusCode = exception switch
			{
				ValidationException => StatusCodes.Status400BadRequest,
				ArgumentException => StatusCodes.Status400BadRequest,
				InvalidOperationException => StatusCodes.Status400BadRequest,
				UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
				KeyNotFoundException => StatusCodes.Status404NotFound,
				_ => StatusCodes.Status500InternalServerError
			};

			if (statusCode >= StatusCodes.Status500InternalServerError)
			{
				_logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
			}
			else
			{
				_logger.LogWarning(exception, "Handled API exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
			}

			httpContext.Response.StatusCode = statusCode;
			httpContext.Response.ContentType = "application/problem+json";

			ProblemDetails problemDetails;
			if (exception is ValidationException validationException)
			{
				var errors = validationException.Errors
					.GroupBy(error => error.PropertyName)
					.ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

				problemDetails = new ValidationProblemDetails(errors)
				{
					Status = statusCode,
					Title = "One or more validation errors occurred.",
					Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
					Instance = httpContext.Request.Path
				};
			}
			else
			{
				problemDetails = new ProblemDetails
				{
					Status = statusCode,
					Title = statusCode >= StatusCodes.Status500InternalServerError ? "Unexpected server error" : exception.Message,
					Type = $"https://tools.ietf.org/html/rfc9110#section-15.{StatusClass(statusCode)}",
					Instance = httpContext.Request.Path
				};
			}

			problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
			await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
			return true;
		}

		private static string StatusClass(int statusCode)
		{
			return statusCode >= 500 ? "6.1" : "5.1";
		}
	}
}
