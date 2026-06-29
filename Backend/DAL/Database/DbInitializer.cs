using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Database
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // Create database if it doesn't exist
                await context.Database.MigrateAsync();

                // Seed roles
                var roles = new[] { "Admin", "User", "PaidUser" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Seed subscription plans
                if (!await context.SubscriptionPlans.AnyAsync())
                {
                    var plans = new[]
                    {
                        new SubscriptionPlan
                        {
                            Name = "Monthly Plan",
                            Description = "Get 1000 session minutes per month",
                            Price = 10m,
                            DurationInDays = 30,
                            SessionMinutes = 1000,
                            PlanType = PlanType.Monthly,
                            IsActive = true,
                            CreatedOn = DateTime.UtcNow
                        },
                        new SubscriptionPlan
                        {
                            Name = "Yearly Plan",
                            Description = "Get 15000 session minutes per year (2 free months)",
                            Price = 100m,
                            DurationInDays = 365,
                            SessionMinutes = 15000,
                            PlanType = PlanType.Yearly,
                            IsActive = true,
                            CreatedOn = DateTime.UtcNow
                        }
                    };
                    await context.SubscriptionPlans.AddRangeAsync(plans);
                    await context.SaveChangesAsync();
                }

                // NOTE: Countries, Governorates and Cities entities were removed in favor of string fields on User (Country, State, City).
                // Any previous seeding for those tables has been intentionally removed. If you need to persist a list of locations,
                // maintain an external service or separate data source.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
            }
        }
    }
}
