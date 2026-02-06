using API.Entities;

namespace API.Interfaces;

public interface IMemberRepository
{
    public void Update(Member member);
    public Task<bool> SaveAllAsync();
    public Task<IReadOnlyList<Member>> GetMembersAsync();
    public Task<Member?> GetMemberByIdAsync(string id);
    public Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId);
    public Task<Member?> GetMemberForUpdate(string id);
}
