using BL.Extensions;
using BL.Mapper;
using BL.Services;
using BL.Services.Abstraction;
using BL.Services.Implementation;
using DAL.Database;
using DAL.Entities;
using DAL.Extensions; // Essential for AddMergedDatabase
using DAL.Repositories;
using DAL.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Load and Verify Connection String
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured in appsettings.json");
            }

            // 2. Register Database via your DAL Extension Method
            Console.WriteLine(connectionString);
            builder.Services.AddMergedDatabase(connectionString);

            // 3. Add Identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // 4. JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            // Default token expiration: 1 day (1440 minutes) if not configured
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440");

            var key = Encoding.ASCII.GetBytes(secretKey ?? throw new InvalidOperationException("JWT SecretKey is missing"));

            // 5. Add Authentication (JWT + Socials)
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                options.Scope.Add("profile");
                options.Scope.Add("email");
            })
            .AddGitHub(options =>
            {
                options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
                options.Scope.Add("user:email");
            });

            // 6. Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", corsBuilder =>
                {
                    corsBuilder
                        .WithOrigins("http://localhost:3000", "http://localhost:4200", "http://127.0.0.1:3000", "http://127.0.0.1:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // 7. Add AutoMapper
            builder.Services.AddAutoMapper(m => m.AddProfile(new MappingProfile()));

            // 8. Add Business Logic Services
            builder.Services.AddServices();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            builder.Services.AddScoped<IJwtTokenService>(provider =>
                new JwtTokenService(secretKey, issuer, audience, expirationMinutes, provider.GetRequiredService<UserManager<User>>()));
            builder.Services.AddScoped<CVService>();

            // 9. MVC & Tooling
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddHostedService<PL.Services.SubscriptionExpirationHostedService>();
            builder.Services.AddScoped<DataMigrationUtility>();
            builder.Services.AddScoped<GraduationProjectRepository>();
            var app = builder.Build();

            // 10. Automated Migration & Seeding Lifecycle Execution
            try
            {
                app.Services.EnsureDatabaseCreatedAndMigrated();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<AppDbContext>();
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();


                    await DbInitializer.InitializeAsync(context, userManager, roleManager);
                }
                Console.WriteLine("✓ Database initialized and custom seeding completed.");
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogCritical(ex, "An unhandled error occurred during pipeline database migration updates.");
            }

            // 11. Pipeline Middlewares
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}