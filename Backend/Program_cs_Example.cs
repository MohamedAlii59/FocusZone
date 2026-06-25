/*
 * EXAMPLE PROGRAM.CS CONFIGURATION
 * This shows how to configure the merged database in your ASP.NET Core application
 * 
 * This is a reference - update your actual Program.cs with these settings
 */

using DAL.Database;
using DAL.Extensions;
using Microsoft.AspNetCore.Identity;
using DAL.Entities;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DATABASE CONFIGURATION
// ============================================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add the merged database with all repositories
builder.Services.AddMergedDatabase(connectionString);

// ============================================================
// IDENTITY CONFIGURATION
// ============================================================

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Configure password requirements as needed
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ============================================================
// ADD OTHER SERVICES
// ============================================================

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================
// BUILD AND CONFIGURE APP
// ============================================================

var app = builder.Build();

// ============================================================
// APPLY MIGRATIONS AND SEED DATA
// ============================================================

try
{
    // Ensure database is created and migrations are applied
    app.Services.EnsureDatabaseCreatedAndMigrated();
    Console.WriteLine("✓ Database migrations applied successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Database migration failed: {ex.Message}");
    throw;
}

// ============================================================
// MIDDLEWARE CONFIGURATION
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/*
 * APPSETTINGS.JSON CONFIGURATION
 * Make sure your appsettings.json has the correct connection string:
 * 
 * {
 *   "ConnectionStrings": {
 *     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=StuckIn;Trusted_Connection=true;TrustServerCertificate=true;"
 *   },
 *   "Logging": {
 *     "LogLevel": {
 *       "Default": "Information"
 *     }
 *   }
 * }
 */
