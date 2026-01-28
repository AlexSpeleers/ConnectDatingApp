using API.DTOs;
using API.Entities;
using System.Security.Cryptography;
using System.Text.Json;

namespace API.Data;

public class Seed
{
    public static async Task SeedData(AppDbContext context)
    {
        if (context.Members.Any()) return;
        string seedText = await File.ReadAllTextAsync("Data/UserSeedData.json");
        List<SeedUserDto>? seedMembers = JsonSerializer.Deserialize<List<SeedUserDto>>(seedText);
        if (seedMembers is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No members in seed data");
            return;
        }

        foreach (var seedMember in seedMembers)
        {
            using var hmac = new HMACSHA512();
            AppUser user = new()
            {
                Id = seedMember.Id,
                DisplayName = seedMember.DisplayName,
                Email = seedMember.Email,
                ImageUrl = seedMember.ImageUrl,
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Pa$$w0rd")),
                PasswordSalt = hmac.Key,
                Member = new()
                {
                    Id = seedMember.Id,
                    DisplayName = seedMember.DisplayName,
                    DateOfBirth = seedMember.DateOfBirth,
                    ImageUrl = seedMember.ImageUrl,
                    Gender = seedMember.Gender,
                    City = seedMember.City,
                    Country = seedMember.Country,
                    Description = seedMember.Description,
                    Created = seedMember.Created,
                    LastActive = seedMember.LastActive,
                }
            };
            user.Member.Photos.Add(new()
            {
                Url = seedMember.ImageUrl!,
                MemberId = seedMember.Id,
            });

            context.Users.Add(user);
        }
        await context.SaveChangesAsync();
    }
}