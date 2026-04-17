using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
    public async void AddLike(MemberLike like) => await context.Likes.AddAsync(like);

    public void DeletLike(MemberLike like) => context.Likes.Remove(like);

    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
    {
        return await context.Likes
            .Where(x => x.SourceMemberId == memberId)
            .Select(x => x.TargetMemberId)
            .ToListAsync();
    }

    public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId) =>
        await context.Likes.FindAsync(sourceMemberId, targetMemberId);

    public async Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams)
    {
        IQueryable<MemberLike> likesQuery = context.Likes.AsQueryable();
        IQueryable<Member> result;
        switch (likesParams.Predicate)
        {
            case "liked":
                result = likesQuery
                    .Where(x => x.SourceMemberId == likesParams.MemberId)
                    .Select(x => x.TargetMember);
                break;
            case "likedBy":
                result = likesQuery
                    .Where(x => x.TargetMemberId == likesParams.MemberId)
                    .Select(x => x.SourceMember);
                break;
            default:
                var likeIds = await GetCurrentMemberLikeIds(likesParams.MemberId);
                result = likesQuery
                    .Where(x => x.TargetMemberId == likesParams.MemberId
                    && likeIds.Contains(x.SourceMemberId))
                    .Select(x => x.SourceMember);
                break;
        }
        return await PaginationHelper.CreateAsync(result, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<bool> SaveAllChanges() => await context.SaveChangesAsync() > 0;
}