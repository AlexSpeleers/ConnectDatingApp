using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MembersController(IMemberRepository memberRepository, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
    {
        return Ok(await memberRepository.GetMembersAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMember(string id)
    {
        Member? member = await memberRepository.GetMemberByIdAsync(id);
        if (member == null) return NotFound();
        return Ok(member);
    }

    [HttpGet("{id}/photos")]
    public async Task<ActionResult<IReadOnlyList<Photo>>> GetPhotosForMember(string id)
    {
        return Ok(await memberRepository.GetPhotosForMemberAsync(id));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        string? memberId = User.GetMemberId();
        Member? member = await memberRepository.GetMemberForUpdate(memberId);
        if (member is null) return BadRequest("Could not find member.");
        member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
        member.Description = memberUpdateDto.Description ?? member.Description;
        member.Country = memberUpdateDto.Country ?? member.Country;
        member.City = memberUpdateDto.City ?? member.City;
        member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;
        memberRepository.Update(member);
        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to update member.");
        return NoContent();
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto([FromForm] IFormFile file)
    {
        Member? member = await memberRepository.GetMemberForUpdate(User.GetMemberId());
        if (member is null) return BadRequest("Could not find member.");
        var uploadResult = await photoService.UploadPhotoAsync(file);
        if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);
        Photo photo = new()
        {
            Url = uploadResult.SecureUrl.AbsoluteUri,
            PublicId = uploadResult.PublicId,
            MemberId = User.GetMemberId()
        };
        if (member.ImageUrl == null)
        {
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
        }
        member.Photos.Add(photo);
        if (!await memberRepository.SaveAllAsync()) return BadRequest("Failed to add photo.");
        return Ok(photo);
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        Member? member = await memberRepository.GetMemberForUpdate(User.GetMemberId());
        if (member is null) return BadRequest("Cannot get member from token.");

        Photo photo = member.Photos.SingleOrDefault(x => x.Id == photoId)!;
        if (member.ImageUrl == photo?.Url || photo == null)
            return BadRequest("Cannot set this as main image.");

        member.ImageUrl = photo?.Url;
        member.User.ImageUrl = photo?.Url;
        if (await memberRepository.SaveAllAsync())
            return NoContent();
        return BadRequest("Problem setting main photo.");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        Member? member = await memberRepository.GetMemberForUpdate(User.GetMemberId());
        if (member is null) return BadRequest("Cannot get member from token.");

        Photo photo = member.Photos.SingleOrDefault(x => x.Id == photoId)!;
        if (photo is null || photo.Url == member.ImageUrl) return BadRequest("This photo cannot be deleted.");

        if (photo.PublicId is not null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }
        member.Photos.Remove(photo);
        if (await memberRepository.SaveAllAsync()) return Ok();
        return BadRequest("Problem deleting the photo.");
    }
}