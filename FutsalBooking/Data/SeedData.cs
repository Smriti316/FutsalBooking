using FutsalBooking.Models;
using Microsoft.AspNetCore.Identity;

namespace FutsalBooking.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)

        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if(!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            if (await userManager.FindByEmailAsync("admin@sglory.com") == null)
            {
                var admin = new User
                {
                    FullName = "Admin",
                    UserName = "admin@sglory.com",
                    Email = "admin@sglory.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // create court
            if (!context.Courts.Any())
            {
                context.Courts.Add(new Court
                {
                    Name = "SGlory Futsal Court",
                    PricePerHour = 1500,
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
