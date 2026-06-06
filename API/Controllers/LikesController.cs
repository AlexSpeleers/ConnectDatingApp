using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(IUnitOfWork uow) : BaseApiController
{
    [HttpPost("{targetMemberId}")]
    public async Task<ActionResult> ToggleLike(string targetMemberId)
    {
        string sourceMemberId = User.GetMemberId();
        if (sourceMemberId == targetMemberId)
            return BadRequest("You cannot like yourself");

        MemberLike? existingLike = await uow.LikesRepository.GetMemberLike(sourceMemberId, targetMemberId);
        if (existingLike is null)
        {
            MemberLike like = new()
            {
                SourceMemberId = sourceMemberId,
                TargetMemberId = targetMemberId
            };
            uow.LikesRepository.AddLike(like);
        }
        else
            uow.LikesRepository.DeleteLike(existingLike);
        if (await uow.CompleteAsync())
            return Ok();
        return BadRequest("Failed to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCurrentMemberLikeIds() => Ok(await uow.LikesRepository.GetCurrentMemberLikeIds(User.GetMemberId()));

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<Member>>> GetMemberLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.MemberId = User.GetMemberId();
        PaginatedResult<Member> members = await uow.LikesRepository.GetMemberLikes(likesParams);
        return Ok(members);
    }
}