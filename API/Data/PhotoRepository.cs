using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(AppDbContext context) : IPhotoRepository
{
    public async Task<IReadOnlyList<PhotoForApprovalDto>> GetUnapprovedPhotos() =>
        await context.Photos
        .IgnoreQueryFilters()
        .Where(photo => photo.IsApproved == false)
        .Select(photo => new PhotoForApprovalDto
        {
            Id = photo.Id,
            Url = photo.Url,
            UserId = photo.MemberId,
            IsApproved = photo.IsApproved
        })
        .ToListAsync();

    public async Task<Photo?> GetPhotoById(int photoId)
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(photo => photo.Id == photoId);
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}