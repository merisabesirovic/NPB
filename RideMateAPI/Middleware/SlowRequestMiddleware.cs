using System.Diagnostics;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RideMateAPI.Middleware
{
	public class SlowRequestMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<SlowRequestMiddleware> _logger;
		private const int ThresholdMs = 1500;

		public SlowRequestMiddleware(RequestDelegate next, ILogger<SlowRequestMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				await _next(context);
			}
			finally
			{
				sw.Stop();
				if (sw.ElapsedMilliseconds > ThresholdMs)
				{
					var path = context.Request?.Path.ToString() ?? "<unknown>";
					var method = context.Request?.Method ?? "<unknown>";
					string userId = "anonymous";
					try
					{
						if (context.User?.Identity?.IsAuthenticated == true)
						{
							userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
								?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
								?? "unknown";
						}
					}
					catch
					{
						// ignore claim extraction errors
					}

					_logger.LogWarning("Slow request detected. Endpoint: {Endpoint}, UserId: {UserId}, Method: {Method}, DurationMs: {DurationMs}",
						path, userId, method, sw.ElapsedMilliseconds);
				}
			}
		}
	}

	public static class SlowRequestMiddlewareExtensions
	{
		public static IApplicationBuilder UseSlowRequestMiddleware(this IApplicationBuilder app)
		{
			return app.UseMiddleware<SlowRequestMiddleware>();
		}
	}
}
