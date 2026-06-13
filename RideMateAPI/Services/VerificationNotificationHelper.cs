using RideMateAPI.Data;
using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public static class VerificationNotificationHelper
	{
		public static string DocumentDisplayName(DocumentType documentType)
		{
			return documentType switch
			{
				DocumentType.DriverLicense => "vozacka dozvola",
				DocumentType.RegistrationCertificate => "saobracajna dozvola",
				DocumentType.IdentityCard => "ID dokument vozaca",
				DocumentType.PassengerIdentityCard => "ID dokument putnika",
				_ => "dokument"
			};
		}

		public static void AddDocumentApprovedNotification(RideMateDbContext db, DriverVerificationDocument document)
		{
			var documentName = DocumentDisplayName(document.DocumentType);
			var message = document.DocumentType == DocumentType.PassengerIdentityCard
				? "Admin je odobrio vas ID dokument. Profil je sada verifikovan i prikazuje se zeleni bedz pored imena."
				: $"Admin je odobrio dokument: {documentName}. Vozacki profil je sada verifikovan.";

			db.Notifications.Add(new Notification
			{
				Id = Guid.NewGuid(),
				UserId = document.UserId,
				Title = "Dokument je odobren",
				Message = message,
				IsRead = false,
				CreatedAt = DateTime.UtcNow
			});
		}

		public static void AddVehicleApprovedNotification(RideMateDbContext db, Vehicle vehicle)
		{
			var vehicleName = string.IsNullOrWhiteSpace(vehicle.Model) ? "vozilo" : vehicle.Model;
			db.Notifications.Add(new Notification
			{
				Id = Guid.NewGuid(),
				UserId = vehicle.UserId,
				Title = "Vozilo je verifikovano",
				Message = $"Admin je verifikovao vozilo {vehicleName}. Vozilo je sada prikazano kao verifikovano na profilu.",
				IsRead = false,
				CreatedAt = DateTime.UtcNow
			});
		}
	}
}
