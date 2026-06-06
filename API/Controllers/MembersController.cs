using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MembersController(IUnitOfWork uow, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers([FromQuery] MemberParams memberParams)
    {
        memberParams.CurrentMemberId = User.GetMemberId();
        return Ok(await uow.MemberRepository.GetMembersAsync(memberParams));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(string id)
    {
        Member? member = await uow.MemberRepository.GetMemberByIdAsync(id);
        if (member == null) return NotFound();
        return Ok(member);
    }

    [HttpGet("{id}/photos")]
    public async Task<ActionResult<IReadOnlyList<Photo>>> GetPhotosForMember(string id)
    {
        bool isCurrentUser = id == User.GetMemberId();
        return Ok(await uow.MemberRepository.GetPhotosForMemberAsync(id, isCurrentUser));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        string? memberId = User.GetMemberId();
        Member? member = await uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member is null) return BadRequest("Could not find member.");
        member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
        member.Description = memberUpdateDto.Description ?? member.Description;
        member.Country = memberUpdateDto.Country ?? member.Country;
        member.City = memberUpdateDto.City ?? member.City;
        member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;
        uow.MemberRepository.Update(member);
        if (!await uow.CompleteAsync()) return BadRequest("Failed to update member.");
        return NoContent();
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto([FromForm] IFormFile file)
    {
        Member? member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());
        if (member is null) return BadRequest("Could not find member.");
        var uploadResult = await photoService.UploadPhotoAsync(file);
        if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);
        Photo photo = new()
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            PublicId = uploadResult.PublicId,
            MemberId = User.GetMemberId()
        };
        member.Photos.Add(photo);
        if (!await uow.CompleteAsync()) return BadRequest("Failed to add photo.");
        return Ok(photo);
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        Member? member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());
        if (member is null) return BadRequest("Cannot get member from token.");

        Photo photo = member.Photos.SingleOrDefault(x => x.Id == photoId)!;
        if (member.ImageUrl == photo?.Url || photo == null)
            return BadRequest("Cannot set this as main image.");

        member.ImageUrl = photo?.Url;
        member.User.ImageUrl = photo?.Url;
        if (await uow.CompleteAsync())
            return NoContent();
        return BadRequest("Problem setting main photo.");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        Member? member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());
        if (member is null) return BadRequest("Cannot get member from token.");

        Photo photo = member.Photos.SingleOrDefault(x => x.Id == photoId)!;
        if (photo is null || photo.Url == member.ImageUrl) return BadRequest("This photo cannot be deleted.");

        if (photo.PublicId is not null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }
        member.Photos.Remove(photo);
        if (await uow.CompleteAsync()) return Ok();
        return BadRequest("Problem deleting the photo.");
    }
}