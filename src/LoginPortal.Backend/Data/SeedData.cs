using Microsoft.AspNetCore.Identity;

namespace LoginPortal.Backend.Data;

public static class SeedData
{
    private static readonly string[] Roles = ["Admin", "User"];

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await CreateUserAsync(userManager, "admin", "admin@admin", "Admin123!", ["Admin", "User"]);
        await CreateUserAsync(userManager, "user", "user@user", "User123!", ["User"]);
    }

    private static async Task CreateUserAsync(
        UserManager<IdentityUser> userManager,
        string username,
        string email,
        string password,
        string[] roles)
    {
        if (await userManager.FindByNameAsync(username) is not null)
            return;

        var user = new IdentityUser { UserName = username, Email = email };
        await userManager.CreateAsync(user, password);

        foreach (var role in roles)
            await userManager.AddToRoleAsync(user, role);
    }
}
