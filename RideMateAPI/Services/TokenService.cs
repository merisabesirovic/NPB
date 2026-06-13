using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public class TokenService
	{
		private readonly IConfiguration _config;

		public TokenService(IConfiguration config)
		{
			_config = config;
		}

		public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, UserRole effectiveRoles)
		{
			var jwtKey = _config["Jwt:Key"] ?? "super_secret_development_key_change_this";
			var jwtIssuer = _config["Jwt:Issuer"] ?? "RideMateApi";
			var jwtAudience = _config["Jwt:Audience"] ?? "RideMateApiClients";
			var accessTokenMinutes = GetInt("Jwt:AccessTokenMinutes", 10080);

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expiresAt = DateTime.UtcNow.AddMinutes(accessTokenMinutes);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(ClaimTypes.Name, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim("roles", ((int)effectiveRoles).ToString())
			};

			if (effectiveRoles.HasFlag(UserRole.Admin)) claims.Add(new Claim(ClaimTypes.Role, "Admin"));
			if (effectiveRoles.HasFlag(UserRole.Driver)) claims.Add(new Claim(ClaimTypes.Role, "Driver"));
			if (effectiveRoles.HasFlag(UserRole.Passenger)) claims.Add(new Claim(ClaimTypes.Role, "Passenger"));

			var token = new JwtSecurityToken(
				issuer: jwtIssuer,
				audience: jwtAudience,
				claims: claims,
				expires: expiresAt,
				signingCredentials: creds);

			return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
		}

		public (string Token, string TokenHash, DateTime ExpiresAt) GenerateRefreshToken()
		{
			var tokenBytes = RandomNumberGenerator.GetBytes(64);
			var token = Convert.ToBase64String(tokenBytes);
			return (token, HashRefreshToken(token), DateTime.UtcNow.AddDays(GetInt("Jwt:RefreshTokenDays", 30)));
		}

		public string HashRefreshToken(string refreshToken)
		{
			var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
			return Convert.ToHexString(hashBytes);
		}

		private int GetInt(string key, int fallback)
		{
			return int.TryParse(_config[key], out var value) && value > 0 ? value : fallback;
		}
	}
}
