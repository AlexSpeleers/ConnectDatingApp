using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config)
    {
        Account account = new(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
        _cloudinary = new(account);
    }
    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        return await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
    {
        ImageUploadResult uploadResult = new();
        if (file.Length > 0)
        {
            await using Stream stream = file.OpenReadStream();
            ImageUploadParams uploadParams = new()
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = "photos of an dating app"
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }
        return uploadResult;
    }
}