using RideMateAPI.Models;

namespace RideMateAPI.Services
{
	public static class UserRoleHelper
	{
		public static bool IsDriverVerificationDocument(DriverVerificationDocument document)
		{
			return document.DocumentType != DocumentType.PassengerIdentityCard;
		}

		public static bool IsPassengerIdentityDocument(DriverVerificationDocument document)
		{
			return document.DocumentType == DocumentType.PassengerIdentityCard;
		}

		public static bool HasApprovedDriverVerification(User user)
		{
			return user.Roles.HasFlag(UserRole.Driver)
				&& user.DriverVerificationDocuments.Any(d => IsDriverVerificationDocument(d) && d.VerificationStatus == VerificationStatus.Approved);
		}

		public static bool HasPendingDriverVerification(User user)
		{
			return user.Roles.HasFlag(UserRole.Driver)
				&& !HasApprovedDriverVerification(user)
				&& user.DriverVerificationDocuments.Any(d => IsDriverVerificationDocument(d) && d.VerificationStatus == VerificationStatus.Pending);
		}

		public static string DriverVerificationStatus(User user)
		{
			if (HasApprovedDriverVerification(user)) return "Approved";
			if (HasPendingDriverVerification(user)) return "Pending";
			if (user.Roles.HasFlag(UserRole.Driver)
				&& user.DriverVerificationDocuments.Any(d => IsDriverVerificationDocument(d) && d.VerificationStatus == VerificationStatus.Rejected)) return "Rejected";
			return "NotRequested";
		}

		public static bool HasPendingIdentityVerification(User user)
		{
			return user.DriverVerificationDocuments.Any(d => d.VerificationStatus == VerificationStatus.Pending);
		}

		public static bool HasPendingPassengerIdentityVerification(User user)
		{
			return user.DriverVerificationDocuments.Any(d => IsPassengerIdentityDocument(d) && d.VerificationStatus == VerificationStatus.Pending);
		}

		public static bool HasApprovedIdentityVerification(User user)
		{
			return user.IsVerified
				|| user.DriverVerificationDocuments.Any(d => d.VerificationStatus == VerificationStatus.Approved);
		}

		public static bool HasOtherApprovedIdentityDocument(User user, Guid excludedDocumentId)
		{
			return user.DriverVerificationDocuments.Any(d => d.Id != excludedDocumentId && d.VerificationStatus == VerificationStatus.Approved);
		}

		public static UserRole EffectiveRoles(User user)
		{
			if (user.Roles.HasFlag(UserRole.Admin)) return UserRole.Admin;

			var roles = user.Roles;
			if (HasApprovedDriverVerification(user))
			{
				roles |= UserRole.Driver;
			}
			else
			{
				roles &= ~UserRole.Driver;
			}

			return roles;
		}
	}
}
