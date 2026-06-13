using MediatR;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.Models;

namespace RideMateAPI.Application.Admin
{
	public record GetDriverRequestsQuery() : IRequest<object>;
	public record GetVehicleVerificationRequestsQuery() : IRequest<object>;

	public class GetDriverRequestsQueryHandler : IRequestHandler<GetDriverRequestsQuery, object>
	{
		private readonly RideMateDbContext _db;

		public GetDriverRequestsQueryHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object> Handle(GetDriverRequestsQuery request, CancellationToken cancellationToken)
		{
			var pending = await _db.DriverVerificationDocuments
				.Where(d => d.VerificationStatus == VerificationStatus.Pending)
				.Include(d => d.User)
				.ToListAsync(cancellationToken);

			return pending.Select(d => new
			{
				DocumentId = d.Id,
				d.UserId,
				UserEmail = d.User?.Email,
				UserFirstName = d.User?.FirstName,
				UserLastName = d.User?.LastName,
				d.DocumentType,
				d.FileUrl,
				d.UploadedAt
			}).ToList();
		}
	}

	public class GetVehicleVerificationRequestsQueryHandler : IRequestHandler<GetVehicleVerificationRequestsQuery, object>
	{
		private readonly RideMateDbContext _db;

		public GetVehicleVerificationRequestsQueryHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object> Handle(GetVehicleVerificationRequestsQuery request, CancellationToken cancellationToken)
		{
			var vehicles = await _db.Vehicles
				.Include(v => v.User)
				.Where(v => !v.IsVerified)
				.OrderBy(v => v.Model)
				.ToListAsync(cancellationToken);

			return vehicles.Select(v => new
			{
				VehicleId = v.Id,
				v.UserId,
				UserEmail = v.User?.Email,
				UserFirstName = v.User?.FirstName,
				UserLastName = v.User?.LastName,
				v.Model,
				v.Year,
				v.SeatsCount,
				v.VehicleImageUrl,
				v.LicenseNumber,
				v.RegistrationCertificateUrl,
				v.IsVerified
			}).ToList();
		}
	}
}
