using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.DTOs;

namespace RideMateAPI.Application.Auth
{
	public record GetCurrentUserQuery(Guid UserId) : IRequest<AuthUserResponse?>;

	public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, AuthUserResponse?>
	{
		private readonly RideMateDbContext _db;
		private readonly IMapper _mapper;

		public GetCurrentUserQueryHandler(RideMateDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public async Task<AuthUserResponse?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
		{
			var user = await _db.Users
				.Include(u => u.DriverVerificationDocuments)
				.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

			return user == null ? null : _mapper.Map<AuthUserResponse>(user);
		}
	}
}
