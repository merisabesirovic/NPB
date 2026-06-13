using MediatR;
using Microsoft.EntityFrameworkCore;
using RideMateAPI.Data;
using RideMateAPI.Models;

namespace RideMateAPI.Application.Verification
{
	public record GetPendingVerificationDocumentsQuery() : IRequest<object>;

	public class GetPendingVerificationDocumentsQueryHandler : IRequestHandler<GetPendingVerificationDocumentsQuery, object>
	{
		private readonly RideMateDbContext _db;

		public GetPendingVerificationDocumentsQueryHandler(RideMateDbContext db)
		{
			_db = db;
		}

		public async Task<object> Handle(GetPendingVerificationDocumentsQuery request, CancellationToken cancellationToken)
		{
			var items = await _db.DriverVerificationDocuments
				.Include(d => d.User)
				.Where(d => d.VerificationStatus == VerificationStatus.Pending)
				.ToListAsync(cancellationToken);

			return items.Select(d => new { d.Id, d.UserId, d.User.Email, d.FileUrl, d.UploadedAt, d.DocumentType }).ToList();
		}
	}
}
