using MediatR;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.Models;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Admin
{
	public record ApproveDriverRequestCommand(Guid DocumentId) : IRequest<object?>;
	public record RejectDriverRequestCommand(Guid DocumentId) : IRequest<object?>;
	public record VerifyVehicleCommand(Guid VehicleId) : IRequest<object?>;

	public class ApproveDriverRequestCommandHandler : IRequestHandler<ApproveDriverRequestCommand, object?>
	{
		private readonly RideMateDbContext _db;

		public ApproveDriverRequestCommandHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object?> Handle(ApproveDriverRequestCommand request, CancellationToken cancellationToken)
		{
			var doc = await _db.DriverVerificationDocuments
				.Include(d => d.User)
				.ThenInclude(u => u.DriverVerificationDocuments)
				.FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);
			if (doc == null) return null;
			if (doc.VerificationStatus == VerificationStatus.Approved) throw new InvalidOperationException("Already approved");

			doc.VerificationStatus = VerificationStatus.Approved;
			if (doc.User != null)
			{
				doc.User.IsVerified = true;
				if (UserRoleHelper.IsDriverVerificationDocument(doc))
				{
					doc.User.Roles |= UserRole.Driver;
				}
				VerificationNotificationHelper.AddDocumentApprovedNotification(_db, doc);
			}

			await _db.SaveChangesAsync(cancellationToken);
			return new { doc.Id, doc.VerificationStatus };
		}
	}

	public class RejectDriverRequestCommandHandler : IRequestHandler<RejectDriverRequestCommand, object?>
	{
		private readonly RideMateDbContext _db;

		public RejectDriverRequestCommandHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object?> Handle(RejectDriverRequestCommand request, CancellationToken cancellationToken)
		{
			var doc = await _db.DriverVerificationDocuments
				.Include(d => d.User)
				.ThenInclude(u => u.DriverVerificationDocuments)
				.FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);
			if (doc == null) return null;
			if (doc.VerificationStatus == VerificationStatus.Rejected) throw new InvalidOperationException("Already rejected");

			doc.VerificationStatus = VerificationStatus.Rejected;
			if (doc.User != null)
			{
				doc.User.IsVerified = UserRoleHelper.HasOtherApprovedIdentityDocument(doc.User, doc.Id);
				if (UserRoleHelper.IsDriverVerificationDocument(doc)
					&& !doc.User.DriverVerificationDocuments.Any(d => d.Id != doc.Id && UserRoleHelper.IsDriverVerificationDocument(d) && d.VerificationStatus == VerificationStatus.Approved))
				{
					doc.User.Roles &= ~UserRole.Driver;
				}
			}

			await _db.SaveChangesAsync(cancellationToken);
			return new { doc.Id, doc.VerificationStatus };
		}
	}

	public class VerifyVehicleCommandHandler : IRequestHandler<VerifyVehicleCommand, object?>
	{
		private readonly RideMateDbContext _db;

		public VerifyVehicleCommandHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object?> Handle(VerifyVehicleCommand request, CancellationToken cancellationToken)
		{
			var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken);
			if (vehicle == null) return null;
			if (vehicle.IsVerified) throw new InvalidOperationException("Vehicle already verified");

			vehicle.IsVerified = true;
			VerificationNotificationHelper.AddVehicleApprovedNotification(_db, vehicle);
			await _db.SaveChangesAsync(cancellationToken);

			return new { vehicle.Id, vehicle.IsVerified };
		}
	}
}
