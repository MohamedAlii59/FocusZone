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

                // Seed countries
                if (!await context.Countries.AnyAsync())
                {
                    var countries = new[]
                    {
                        new Country { Name = "Egypt", Code = "EG" },
                        new Country { Name = "Saudi Arabia", Code = "SA" },
                        new Country { Name = "United Arab Emirates", Code = "AE" }
                    };
                    await context.Countries.AddRangeAsync(countries);
                    await context.SaveChangesAsync();
                }

                // Seed governorates for Egypt
                if (!await context.Governorates.AnyAsync())
                {
                    var egypt = await context.Countries.FirstOrDefaultAsync(c => c.Code == "EG");
                    if (egypt != null)
                    {
                        var governorates = new[]
                        {
                            new Governorate { Name = "Cairo", CountryId = egypt.Id },
                            new Governorate { Name = "Alexandria", CountryId = egypt.Id },
                            new Governorate { Name = "Giza", CountryId = egypt.Id },
                            new Governorate { Name = "Aswan", CountryId = egypt.Id }
                        };
                        await context.Governorates.AddRangeAsync(governorates);
                        await context.SaveChangesAsync();
                    }
                }

                // Seed cities
                if (!await context.Cities.AnyAsync())
                {
                    var cairo = await context.Governorates.FirstOrDefaultAsync(g => g.Name == "Cairo");
                    if (cairo != null)
                    {
                        var cities = new[]
                        {
                            new City { Name = "Downtown Cairo", GovernorateId = cairo.Id },
                            new City { Name = "New Cairo", GovernorateId = cairo.Id },
                            new City { Name = "Nasr City", GovernorateId = cairo.Id }
                        };
                        await context.Cities.AddRangeAsync(cities);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
            }
        }
    }
}
