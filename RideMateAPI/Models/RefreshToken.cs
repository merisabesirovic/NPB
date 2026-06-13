namespace RideMateAPI.Models
{
	public class RefreshToken
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string TokenHash { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public DateTime? RevokedAt { get; set; }
		public string CreatedByIp { get; set; } = string.Empty;
		public string RevokedByIp { get; set; } = string.Empty;
		public string ReplacedByTokenHash { get; set; } = string.Empty;
		public User User { get; set; } = null!;
	}
}
