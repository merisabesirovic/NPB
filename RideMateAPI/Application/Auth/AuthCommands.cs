using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.DTOs;
using RideMateAPI.Models;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Auth
{
	public record RegisterCommand(RegisterRequest Request) : IRequest<RegisterResponse>;
	public record LoginCommand(LoginRequest Request, string IpAddress) : IRequest<AuthResponse?>;
	public record RefreshTokenCommand(RefreshTokenRequest Request, string IpAddress) : IRequest<AuthResponse?>;
	public record RevokeRefreshTokenCommand(RevokeRefreshTokenRequest Request, string IpAddress) : IRequest<bool>;

	public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
	{
		private readonly RideMateDbContext _db;
		private readonly IPasswordHasher<User> _passwordHasher;
		private readonly CloudinaryService _cloudinaryService;
		private readonly IMapper _mapper;

		public RegisterCommandHandler(RideMateDbContext db, IPasswordHasher<User> passwordHasher, CloudinaryService cloudinaryService, IMapper mapper)
		{
			_db = db;
			_passwordHasher = passwordHasher;
			_cloudinaryService = cloudinaryService;
			_mapper = mapper;
		}

		public async Task<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
		{
			var req = command.Request;
			var normalizedEmail = req.Email.Trim().ToLower();

			var existing = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
			if (existing)
			{
				throw new InvalidOperationException("Email already in use");
			}

			var user = _mapper.Map<User>(req);
			user.Email = req.Email.Trim();
			user.PasswordHash = _passwordHasher.HashPassword(user, req.Password);

			if (req.RegisterAsDriver)
			{
				var vehicle = _mapper.Map<Vehicle>(req);
				vehicle.UserId = user.Id;
				user.Vehicles.Add(vehicle);
				user.Roles |= UserRole.Driver;
			}

			if (req.Avatar != null)
			{
				user.AvatarUrl = await _cloudinaryService.UploadAsync(req.Avatar, folder: "avatars");
			}

			if (req.IdentityDocumentFile != null)
			{
				var docUrl = await _cloudinaryService.UploadAsync(req.IdentityDocumentFile, folder: "identity_documents");
				user.DriverVerificationDocuments.Add(new DriverVerificationDocument
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					DocumentType = DocumentType.IdentityCard,
					FileUrl = docUrl,
					VerificationStatus = VerificationStatus.Pending,
					UploadedAt = DateTime.UtcNow
				});
			}

			_db.Users.Add(user);
			await _db.SaveChangesAsync(cancellationToken);

			return new RegisterResponse
			{
				Id = user.Id,
				Email = user.Email
			};
		}
	}

	public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse?>
	{
		private readonly RideMateDbContext _db;
		private readonly IPasswordHasher<User> _passwordHasher;
		private readonly AuthSessionService _authSessionService;

		public LoginCommandHandler(RideMateDbContext db, IPasswordHasher<User> passwordHasher, AuthSessionService authSessionService)
		{
			_db = db;
			_passwordHasher = passwordHasher;
			_authSessionService = authSessionService;
		}

		public async Task<AuthResponse?> Handle(LoginCommand command, CancellationToken cancellationToken)
		{
			var email = command.Request.Email.Trim().ToLower();
			var user = await _db.Users
				.Include(u => u.DriverVerificationDocuments)
				.Include(u => u.RefreshTokens)
				.FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);

			if (user == null)
			{
				return null;
			}

			var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Request.Password);
			if (result == PasswordVerificationResult.Failed)
			{
				return null;
			}

			var response = _authSessionService.Issue(user, command.IpAddress);
			await _db.SaveChangesAsync(cancellationToken);
			return response;
		}
	}

	public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse?>
	{
		private readonly RideMateDbContext _db;
		private readonly AuthSessionService _authSessionService;

		public RefreshTokenCommandHandler(RideMateDbContext db, AuthSessionService authSessionService)
		{
			_db = db;
			_authSessionService = authSessionService;
		}

		public async Task<AuthResponse?> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
		{
			var tokenHash = _authSessionService.HashRefreshToken(command.Request.RefreshToken);
			var storedToken = await _db.RefreshTokens
				.Include(rt => rt.User)
					.ThenInclude(u => u.DriverVerificationDocuments)
				.Include(rt => rt.User)
					.ThenInclude(u => u.RefreshTokens)
				.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

			if (storedToken == null || storedToken.RevokedAt.HasValue || storedToken.ExpiresAt <= DateTime.UtcNow)
			{
				return null;
			}

			storedToken.RevokedAt = DateTime.UtcNow;
			storedToken.RevokedByIp = command.IpAddress;

			var response = _authSessionService.Issue(storedToken.User, command.IpAddress);
			storedToken.ReplacedByTokenHash = _authSessionService.HashRefreshToken(response.RefreshToken);

			await _db.SaveChangesAsync(cancellationToken);
			return response;
		}
	}

	public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, bool>
	{
		private readonly RideMateDbContext _db;
		private readonly AuthSessionService _authSessionService;

		public RevokeRefreshTokenCommandHandler(RideMateDbContext db, AuthSessionService authSessionService)
		{
			_db = db;
			_authSessionService = authSessionService;
		}

		public async Task<bool> Handle(RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
		{
			var tokenHash = _authSessionService.HashRefreshToken(command.Request.RefreshToken);
			var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
			if (storedToken == null)
			{
				return false;
			}

			if (!storedToken.RevokedAt.HasValue)
			{
				storedToken.RevokedAt = DateTime.UtcNow;
				storedToken.RevokedByIp = command.IpAddress;
				await _db.SaveChangesAsync(cancellationToken);
			}

			return true;
		}
	}
}
