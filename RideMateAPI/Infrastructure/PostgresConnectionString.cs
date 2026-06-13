using System.Net;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace RideMateAPI.Infrastructure;

public static class PostgresConnectionString
{
	public static string? FromConfiguration(IConfiguration configuration)
	{
		var rawValue =
			Environment.GetEnvironmentVariable("RIDE_MATE_CONNECTION") ??
			Environment.GetEnvironmentVariable("DATABASE_URL") ??
			configuration.GetConnectionString("DefaultConnection");

		return Normalize(rawValue);
	}

	public static string? Normalize(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		var trimmed = value.Trim();
		if (!trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
			!trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
		{
			return trimmed;
		}

		var uri = new Uri(trimmed);
		var userInfo = uri.UserInfo.Split(':', 2);
		var database = uri.AbsolutePath.TrimStart('/');

		var builder = new NpgsqlConnectionStringBuilder
		{
			Host = uri.Host,
			Port = uri.Port > 0 ? uri.Port : 5432,
			Database = WebUtility.UrlDecode(database),
			Username = WebUtility.UrlDecode(userInfo.ElementAtOrDefault(0) ?? string.Empty),
			Password = WebUtility.UrlDecode(userInfo.ElementAtOrDefault(1) ?? string.Empty),
			SslMode = SslMode.Require
		};

		return builder.ConnectionString;
	}
}
