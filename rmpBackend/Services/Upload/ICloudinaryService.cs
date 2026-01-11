namespace rmpBackend.Services.Upload
{
    public interface ICloudinaryService
    {
        Task<string> UploadPdfAsync(IFormFile file, string folder);
    }

}
