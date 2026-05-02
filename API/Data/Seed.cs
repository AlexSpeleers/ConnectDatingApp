using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;

namespace API.Data;

public class Seed
{
    public static async Task SeedData(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;
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
            AppUser user = new()
            {
                Id = seedMember.Id,
                DisplayName = seedMember.DisplayName,
                UserName = seedMember.Email,
                Email = seedMember.Email,
                ImageUrl = seedMember.ImageUrl,
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

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");
            if (!result.Succeeded)
                Console.WriteLine(result.Errors.First().Description);
            await userManager.AddToRoleAsync(user, "Member");
        }

        AppUser admin = new()
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            DisplayName = "Admin"
        };
        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
    }
}