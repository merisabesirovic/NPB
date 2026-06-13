using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.Models;
using RideMateAPI.Services;

namespace RideMateAPI.Application.Verification
{
	public record UploadIdentityDocumentCommand(Guid UserId, IFormFile File) : IRequest<object>;
	public record UploadPassengerIdentityDocumentCommand(Guid UserId, IFormFile File) : IRequest<object>;
	public record ApproveVerificationDocumentCommand(Guid DocumentId) : IRequest<object?>;
	public record RejectVerificationDocumentCommand(Guid DocumentId) : IRequest<object?>;

	public class UploadIdentityDocumentCommandHandler : IRequestHandler<UploadIdentityDocumentCommand, object>
	{
		private readonly RideMateDbContext _db;
		private readonly CloudinaryService _cloudinary;

		public UploadIdentityDocumentCommandHandler(RideMateDbContext db, CloudinaryService cloudinary)
		{
			_db = db;
			_cloudinary = cloudinary;
		}

		public async Task<object> Handle(UploadIdentityDocumentCommand request, CancellationToken cancellationToken)
		{
			if (request.File == null || request.File.Length == 0) throw new ArgumentException("File is required");

			var user = await _db.Users.Include(u => u.DriverVerificationDocuments).FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
			if (user == null) throw new KeyNotFoundException("User not found");

			var url = await _cloudinary.UploadAsync(request.File, folder: "identity_documents");
			var doc = new DriverVerificationDocument
			{
				Id = Guid.NewGuid(),
				UserId = user.Id,
				DocumentType = DocumentType.IdentityCard,
				FileUrl = url,
				VerificationStatus = VerificationStatus.Pending,
				UploadedAt = DateTime.UtcNow
			};

			_db.DriverVerificationDocuments.Add(doc);
			await _db.SaveChangesAsync(cancellationToken);

			var rows = await _db.Users.Where(u => u.Id == user.Id)
				.ExecuteUpdateAsync(s => s.SetProperty(u => u.Roles, u => u.Roles | UserRole.Driver), cancellationToken);
			if (rows == 0)
			{
				throw new InvalidOperationException("Could not update user roles; the user may have been modified or removed. Try again.");
			}

			return new { docId = doc.Id, url };
		}
	}

	public class UploadPassengerIdentityDocumentCommandHandler : IRequestHandler<UploadPassengerIdentityDocumentCommand, object>
	{
		private readonly RideMateDbContext _db;
		private readonly CloudinaryService _cloudinary;

		public UploadPassengerIdentityDocumentCommandHandler(RideMateDbContext db, CloudinaryService cloudinary)
		{
			_db = db;
			_cloudinary = cloudinary;
		}

		public async Task<object> Handle(UploadPassengerIdentityDocumentCommand request, CancellationToken cancellationToken)
		{
			if (request.File == null || request.File.Length == 0) throw new ArgumentException("File is required");

			var user = await _db.Users.Include(u => u.DriverVerificationDocuments).FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
			if (user == null) throw new KeyNotFoundException("User not found");
			if (UserRoleHelper.HasApprovedIdentityVerification(user)) throw new InvalidOperationException("Identity is already verified");
			if (UserRoleHelper.HasPendingPassengerIdentityVerification(user)) throw new InvalidOperationException("Passenger identity document is already pending approval");

			var url = await _cloudinary.UploadAsync(request.File, folder: "identity_documents");
			var doc = new DriverVerificationDocument
			{
				Id = Guid.NewGuid(),
				UserId = user.Id,
				DocumentType = DocumentType.PassengerIdentityCard,
				FileUrl = url,
				VerificationStatus = VerificationStatus.Pending,
				UploadedAt = DateTime.UtcNow
			};

			_db.DriverVerificationDocuments.Add(doc);
			await _db.SaveChangesAsync(cancellationToken);

			return new { docId = doc.Id, url, status = "Pending" };
		}
	}

	public class ApproveVerificationDocumentCommandHandler : IRequestHandler<ApproveVerificationDocumentCommand, object?>
	{
		private readonly RideMateDbContext _db;

		public ApproveVerificationDocumentCommandHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object?> Handle(ApproveVerificationDocumentCommand request, CancellationToken cancellationToken)
		{
			var doc = await _db.DriverVerificationDocuments
				.Include(d => d.User)
				.ThenInclude(u => u.DriverVerificationDocuments)
				.FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);
			if (doc == null) return null;

			var wasAlreadyApproved = doc.VerificationStatus == VerificationStatus.Approved;
			doc.VerificationStatus = VerificationStatus.Approved;
			doc.User.IsVerified = true;
			if (UserRoleHelper.IsDriverVerificationDocument(doc))
			{
				doc.User.Roles |= UserRole.Driver;
			}
			if (!wasAlreadyApproved)
			{
				VerificationNotificationHelper.AddDocumentApprovedNotification(_db, doc);
			}
			await _db.SaveChangesAsync(cancellationToken);

			return new { doc.Id, status = "Approved" };
		}
	}

	public class RejectVerificationDocumentCommandHandler : IRequestHandler<RejectVerificationDocumentCommand, object?>
	{
		private readonly RideMateDbContext _db;

		public RejectVerificationDocumentCommandHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object?> Handle(RejectVerificationDocumentCommand request, CancellationToken cancellationToken)
		{
			var doc = await _db.DriverVerificationDocuments
				.Include(d => d.User)
				.ThenInclude(u => u.DriverVerificationDocuments)
				.FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);
			if (doc == null) return null;

			doc.VerificationStatus = VerificationStatus.Rejected;
			doc.User.IsVerified = UserRoleHelper.HasOtherApprovedIdentityDocument(doc.User, doc.Id);
			if (UserRoleHelper.IsDriverVerificationDocument(doc)
				&& !doc.User.DriverVerificationDocuments.Any(d => d.Id != doc.Id && UserRoleHelper.IsDriverVerificationDocument(d) && d.VerificationStatus == VerificationStatus.Approved))
			{
				doc.User.Roles &= ~UserRole.Driver;
			}
			await _db.SaveChangesAsync(cancellationToken);

			return new { doc.Id, status = "Rejected" };
		}
	}
}
