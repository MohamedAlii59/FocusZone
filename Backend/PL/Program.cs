
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DAL.Entities;
using DAL.Database;
using BL.Services;
using BL.Mapper;
namespace PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured in appsettings.json");
            }

            // Add DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));


            // Add Identity
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

            // JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            var key = Encoding.ASCII.GetBytes(secretKey);

            // Add JWT Bearer Authentication
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

            // Add CORS
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

            // Add AutoMapper
            builder.Services.AddAutoMapper(m=>m.AddProfile(new MappingProfile()));

            // Add Services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            builder.Services.AddScoped<IJwtTokenService>(provider =>
                new JwtTokenService(secretKey, issuer, audience, expirationMinutes, provider.GetRequiredService<UserManager<User>>()));

            // Add Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add Controllers
            builder.Services.AddControllers();

            // Add HttpClient factory for typed/centralized HttpClient usage
            builder.Services.AddHttpClient();

            // Add Hosted Services
            builder.Services.AddHostedService<PL.Services.SubscriptionExpirationHostedService>();

            var app = builder.Build();
            
            // Initialize Database with retry logic
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var maxRetries = 3;
                var retryCount = 0;
                bool initialized = false;

                while (retryCount < maxRetries && !initialized)
                {
                    try
                    {
                        retryCount++;
                        logger.LogInformation($"Database initialization attempt {retryCount}/{maxRetries}...");
                        
                        var context = services.GetRequiredService<AppDbContext>();
                        
                        // Test connection first
                        if (context.Database.CanConnect())
                        {
                            logger.LogInformation("? Database connection successful");
                            
                            var userManager = services.GetRequiredService<UserManager<User>>();
                            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                            logger.LogInformation("Starting database initialization...");
                            DbInitializer.InitializeAsync(context, userManager, roleManager).Wait();
                            logger.LogInformation("? Database initialization completed successfully.");
                            initialized = true;
                        }
                        else
                        {
                            logger.LogWarning($"Cannot connect to database. Retry {retryCount}/{maxRetries}...");
                            if (retryCount < maxRetries)
                                Task.Delay(2000).Wait(); // Wait 2 seconds before retry
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Attempt {retryCount}: Database initialization failed.");
                        logger.LogError($"Connection Error Details: {ex.InnerException?.Message}");
                        
                        if (retryCount < maxRetries)
                        {
                            logger.LogInformation($"Retrying in 3 seconds...");
                            Task.Delay(3000).Wait();
                        }
                        else
                        {
                            logger.LogCritical("? Failed to initialize database after {MaxRetries} attempts", maxRetries);
                            logger.LogCritical("Please ensure SQL Server is running and accessible.");
                            throw;
                        }
                    }
                }
            }
            
            // Configure the HTTP request pipeline
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
