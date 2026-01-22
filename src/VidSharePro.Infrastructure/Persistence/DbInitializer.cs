using VidSharePro.Domain.Entities;
using VidSharePro.Domain.Enums;
using BCrypt.Net;

namespace VidSharePro.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if there are no users
        if (!context.Users.Any())
        {
            var admin = new User(
                "admin",
                "admin@vidsharepro.com",
                BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Default password
                UserRole.Admin
            );

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}