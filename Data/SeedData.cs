using FutsalBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FutsalBooking.Data
{
    // this class runs once when the app starts
    // it creates the default admin account and some courts if db is empty
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // make sure db is created
            context.Database.EnsureCreated();

            // create roles if they dont exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // create default admin user
            string adminEmail = "admin@playzone.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    FullName = "Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var createResult = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // add default courts if none exist
            if (!context.Courts.Any())
            {
                var courts = new List<Court>
                {
                    new Court
                    {
                        Name = "Court 1 - Main Hall",
                        Description = "Main indoor court with artificial turf. Fits 5v5.",
                        PricePerHour = 1500,
                        CourtType = "Indoor",
                        IsActive = true
                    },
                    new Court
                    {
                        Name = "Court 2 - Outdoor",
                        Description = "Open air court. Best for evening games.",
                        PricePerHour = 1000,
                        CourtType = "Outdoor",
                        IsActive = true
                    },
                    new Court
                    {
                        Name = "Court 3 - VIP",
                        Description = "Premium court with good lighting and spectator area.",
                        PricePerHour = 2000,
                        CourtType = "Indoor",
                        IsActive = true
                    },
                    new Court
                    {
                        Name = "Court 4 - Practice",
                        Description = "Smaller court good for training and practice sessions.",
                        PricePerHour = 800,
                        CourtType = "Outdoor",
                        IsActive = true
                    }
                };

                context.Courts.AddRange(courts);
                await context.SaveChangesAsync();
            }
        }
    }
}
