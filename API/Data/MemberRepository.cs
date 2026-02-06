using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id) => await context.Members.FindAsync(id);

    public async Task<IReadOnlyList<Member>> GetMembersAsync() => await context.Members.ToListAsync();

    public async Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId)
    {
        return await context.Members
            .Where(member => member.Id == memberId)
            .SelectMany(x => x.Photos)
            .ToListAsync();
    }

    public async Task<bool> SaveAllAsync() => await context.SaveChangesAsync() > 0;

    public void Update(Member member) => context.Entry(member).State = EntityState.Modified;

    public async Task<Member?> GetMemberForUpdate(string id) => await context.Members.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
}