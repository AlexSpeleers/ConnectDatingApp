using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        string tokenKey = config["TokenKey"] ?? throw new Exception("Cannot get token key");
        if (tokenKey.Length < 64)
            throw new Exception("Your token key needs to be >= 64 char");
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(tokenKey));

        List<Claim> claims = new()
        {
            new (ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier,user.Id)
        };

        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}