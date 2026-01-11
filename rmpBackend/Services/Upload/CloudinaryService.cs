using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using rmpBackend.Models.DTOs;



namespace rmpBackend.Services.Upload
{
   
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadPdfAsync(IFormFile file, string folder)
        {
         
            if (file == null || file.Length == 0)
                throw new Exception("File is empty");

            if (file.ContentType != "application/pdf")
                throw new Exception("Only PDF files are allowed");

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Format = "pdf",        
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Cloudinary upload failed");

            return uploadResult.SecureUrl.AbsoluteUri;


        }
    }

}
