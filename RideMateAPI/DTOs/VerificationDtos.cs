using Microsoft.AspNetCore.Http;

namespace RideMateAPI.DTOs
{
	public class IdentityUploadRequest
	{
		public IFormFile File { get; set; } = null!;
	}
}
