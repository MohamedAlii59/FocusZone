using DAL.Database;
using DAL.Repositories;
using DAL.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DAL.Entities;

namespace DAL.Extensions
{
    /// <summary>
    /// Extension methods for configuring the merged database in dependency injection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the merged database context and repositories to the service collection
        /// </summary>
        public static IServiceCollection AddMergedDatabase(
            this IServiceCollection services,
            string connectionString)
        {
            // Add the merged DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.EnableRetryOnFailure(3);
                }));

            // Add repositories for domain logic
            services.AddScoped<GraduationProjectRepository>();
            services.AddScoped<DataMigrationUtility>();

            return services;
        }

        /// <summary>
        /// Ensure the database is created and migrations are applied
        /// </summary>
        public static void EnsureDatabaseCreatedAndMigrated(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Apply any pending migrations
                context.Database.Migrate();

                // Optionally seed default data
                SeedDefaultData(context);
            }
        }

        /// <summary>
        /// Seed default data if needed (e.g., initial topics, subscription plans)
        /// </summary>
        private static void SeedDefaultData(AppDbContext context)
        {
            if (context.Topics.Any())
                return;

            var domains = new[]
            {
                new { Name = "Programming",    Type = "Domain", Description = "Core programming concepts and paradigms" },
                new { Name = "Database Design", Type = "Domain", Description = "Relational and non-relational database design" },
                new { Name = "Web Development", Type = "Domain", Description = "Frontend and backend web development" },
            };

            foreach (var domain in domains)
            {
                if (!context.Topics.Any(t => t.Name == domain.Name))
                {
                    context.Topics.Add(new Topic
                    {
                        Name = domain.Name,
                        Type = domain.Type,
                        Difficulty = 2,
                        EstimatedHours = 10.00m,
                        Description = domain.Description  // ✅ fixes the NULL error
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
