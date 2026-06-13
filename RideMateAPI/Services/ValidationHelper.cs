using System.Text.RegularExpressions;

namespace RideMateAPI.Services
{
	public static class ValidationHelper
	{
		private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
		private static readonly Regex PasswordUpper = new Regex("[A-Z]", RegexOptions.Compiled);
		private static readonly Regex PasswordLower = new Regex("[a-z]", RegexOptions.Compiled);
		private static readonly Regex PasswordDigit = new Regex("[0-9]", RegexOptions.Compiled);
		private static readonly Regex PasswordSpecial = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);

		public static bool IsValidEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email)) return false;
			return EmailRegex.IsMatch(email);
		}

		// Validates password and returns null if valid, otherwise an error message
		public static string? ValidatePassword(string password)
		{
			if (string.IsNullOrEmpty(password)) return "Password is required";
			if (password.Length < 8) return "Password must be at least 8 characters long";
			if (!PasswordUpper.IsMatch(password)) return "Password must contain at least one uppercase letter";
			if (!PasswordLower.IsMatch(password)) return "Password must contain at least one lowercase letter";
			if (!PasswordDigit.IsMatch(password)) return "Password must contain at least one digit";
			if (!PasswordSpecial.IsMatch(password)) return "Password must contain at least one special character";
			return null;
		}
	}
}
