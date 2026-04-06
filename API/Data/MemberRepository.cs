using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id) => await context.Members.FindAsync(id);

    public async Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams)
    {
        IQueryable<Member> query = context.Members.AsQueryable();
        query = query.Where(x => x.Id != memberParams.CurrentMemberId);
        if (memberParams.Gender != null)
            query = query.Where(x => x.Gender == memberParams.Gender);
        DateOnly minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1));
        DateOnly maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(memberParams.MinAge));
        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
        query = memberParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive)
        };
        return await PaginationHelper.CreateAsync(query, memberParams.PageNumber, memberParams.PageSize);
    }

    public async Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId)
    {
        return await context.Members
            .Where(member => member.Id == memberId)
            .SelectMany(x => x.Photos)
            .ToListAsync();
    }

    public async Task<bool> SaveAllAsync() => await context.SaveChangesAsync() > 0;

    public void Update(Member member) => context.Entry(member).State = EntityState.Modified;

    public async Task<Member?> GetMemberForUpdate(string id) => await context.Members
        .Include(x => x.User)
        .Include(x => x.Photos)
        .SingleOrDefaultAsync(x => x.Id == id);
}