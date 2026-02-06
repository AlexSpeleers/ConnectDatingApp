using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Authorize]
public class MembersController(IMemberRepository memberRepository) : BaseApiController
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
}