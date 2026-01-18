using API.DTOs;
using API.Entities;

namespace API.Extensions;

public static class AppUserExtension
{
    public static UserDto ToDto(this AppUser user, string token)
    {
        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Token = token
        };
    }
}
