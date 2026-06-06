using API.Entities;
using API.Interfaces;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork uow, IPhotoService photoService) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await userManager.Users.OrderByDescending(x => x.UserName).Reverse().ToListAsync();
        var userList = new List<object>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            userList.Add(new
            {
                user.Id,
                user.Email,
                Roles = roles.ToList()
            });
        }
        return Ok(userList);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{userId}")]
    public async Task<ActionResult<IList<string>>> EditRoles(string userId, [FromQuery] string roles)
    {
        if (string.IsNullOrEmpty(roles))
            return BadRequest("You must select at least one role");

        var selectedRoles = roles.Split(',').ToArray();
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return BadRequest("Could not retriev user");

        var userRoles = await userManager.GetRolesAsync(user);
        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (result.Succeeded)
            return BadRequest("Failed to add to roles");

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded)
            return BadRequest("Failed to remove from roles");

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForModeration() =>
        Ok(await uow.PhotoRepository.GetUnapprovedPhotos());

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPut("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        Photo? photo = await uow.PhotoRepository.GetPhotoById(photoId);
        if (photo == null)
            return BadRequest("Could not retrieve photo");

        photo.IsApproved = true;
        Member? member = uow.MemberRepository.GetMemberForUpdate(photo.MemberId).Result;
        if (member is not null && string.IsNullOrEmpty(member.ImageUrl))
        {
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
        }
        await uow.CompleteAsync();
        return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpDelete("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo = await uow.PhotoRepository.GetPhotoById(photoId);
        if (photo == null)
            return BadRequest("Could not retrieve photo");

        if (photo.PublicId != null)
        {
            DeletionResult result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Result == "ok") { 
                uow.PhotoRepository.RemovePhoto(photo);
                await uow.CompleteAsync();
            }
        }
        return Ok();
    }
}