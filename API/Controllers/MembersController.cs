using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class MembersController(AppDbContext context) : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<AppUser>>> GetMembers()
    {
        List<AppUser> members = await context.Users.ToListAsync();
        return Ok(members);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> GetMember(string id)
    {
        AppUser? member = await context.Users.FindAsync(id);
        if (member == null) return NotFound();
        return Ok(member);
    }
}
