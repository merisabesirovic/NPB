using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace RideMateAPI.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService()
        {
            var url = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("CLOUDINARY_URL environment variable is not configured.");
            }

            _cloudinary = new Cloudinary(url);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is missing");

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
            };

            if (!string.IsNullOrWhiteSpace(folder))
                uploadParams.Folder = folder;

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK || uploadResult.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
            }

            var msg = uploadResult.Error?.Message ?? "Cloudinary upload failed";
            throw new InvalidOperationException(msg);
        }
    }
}
