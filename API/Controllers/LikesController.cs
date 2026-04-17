using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(ILikesRepository likesRepository) : BaseApiController
{
    [HttpPost("{targetMemberId}")]
    public async Task<ActionResult> ToggleLike(string targetMemberId)
    {
        string sourceMemberId = User.GetMemberId();
        if (sourceMemberId == targetMemberId)
            return BadRequest("You cannot like yourself");

        MemberLike? existingLike = await likesRepository.GetMemberLike(sourceMemberId, targetMemberId);
        if (existingLike is null)
        {
            MemberLike like = new()
            {
                SourceMemberId = sourceMemberId,
                TargetMemberId = targetMemberId
            };
            likesRepository.AddLike(like);
        }
        else
            likesRepository.DeletLike(existingLike);
        if (await likesRepository.SaveAllChanges())
            return Ok();
        return BadRequest("Failed to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCurrentMemberLikeIds() => Ok(await likesRepository.GetCurrentMemberLikeIds(User.GetMemberId()));

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<Member>>> GetMemberLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.MemberId = User.GetMemberId();
        PaginatedResult<Member> members = await likesRepository.GetMemberLikes(likesParams);
        return Ok(members);
    }
}