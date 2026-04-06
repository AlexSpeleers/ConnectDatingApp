using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMemberRepository
{
    public void Update(Member member);
    public Task<bool> SaveAllAsync();
    public Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams);
    public Task<Member?> GetMemberByIdAsync(string id);
    public Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId);
    public Task<Member?> GetMemberForUpdate(string id);
}
