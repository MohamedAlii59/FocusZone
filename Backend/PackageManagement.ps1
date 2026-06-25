#!/usr/bin/env powershell
# 📦 NuGet Package Management Commands Reference
# For ITI Graduation Project - Database Merge

# ============================================================
# VERIFICATION COMMANDS (Run These First)
# ============================================================

# Check .NET version
Write-Host "Checking .NET version..." -ForegroundColor Cyan
dotnet --version

# Check SDK info
Write-Host "`nChecking .NET SDK info..." -ForegroundColor Cyan
dotnet --info

# ============================================================
# RESTORE & UPDATE COMMANDS
# ============================================================

# Restore all NuGet packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Check for outdated packages
Write-Host "`nChecking for outdated packages..." -ForegroundColor Yellow
dotnet package update --dry-run

# Restore with verbose output
Write-Host "`nRestoring packages (verbose)..." -ForegroundColor Yellow
dotnet restore --verbosity normal

# ============================================================
# ENTITY FRAMEWORK CORE COMMANDS
# ============================================================

# Check EF Core version
Write-Host "`nChecking EF Core version..." -ForegroundColor Cyan
dotnet ef --version

# Update EF Core tools (if needed)
Write-Host "`nUpdating EF Core tools..." -ForegroundColor Yellow
dotnet tool update dotnet-ef --global

# List all migrations
Write-Host "`nListing all migrations..." -ForegroundColor Cyan
dotnet ef migrations list --project DAL --startup-project PL

# ============================================================
# BUILD & CLEAN COMMANDS
# ============================================================

# Build solution
Write-Host "`nBuilding solution..." -ForegroundColor Green
dotnet build

# Clean build
Write-Host "`nCleaning solution..." -ForegroundColor Yellow
dotnet clean

# Clean and restore
Write-Host "`nCleaning and restoring..." -ForegroundColor Yellow
dotnet clean
dotnet restore

# ============================================================
# PROJECT-SPECIFIC RESTORE
# ============================================================

# Restore DAL project only
Write-Host "`nRestoring DAL project..." -ForegroundColor Cyan
dotnet restore DAL/DAL.csproj

# Restore BL project only
Write-Host "`nRestoring BL project..." -ForegroundColor Cyan
dotnet restore BL/BL.csproj

# Restore PL project only
Write-Host "`nRestoring PL project..." -ForegroundColor Cyan
dotnet restore PL/PL.csproj

# ============================================================
# NUGET CACHE MANAGEMENT
# ============================================================

# Clear all local NuGet caches
Write-Host "`nClearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear

# List NuGet local caches
Write-Host "`nListing NuGet caches..." -ForegroundColor Cyan
dotnet nuget locals all --list

# ============================================================
# PACKAGE INFO COMMANDS
# ============================================================

# List project dependencies (DAL)
Write-Host "`nListing DAL dependencies..." -ForegroundColor Cyan
dotnet list DAL/DAL.csproj package

# List project dependencies (BL)
Write-Host "`nListing BL dependencies..." -ForegroundColor Cyan
dotnet list BL/BL.csproj package

# List project dependencies (PL)
Write-Host "`nListing PL dependencies..." -ForegroundColor Cyan
dotnet list PL/PL.csproj package

# List project dependencies (ALL)
Write-Host "`nListing all project dependencies..." -ForegroundColor Cyan
dotnet list package

# ============================================================
# MIGRATION COMMANDS (After Packages Verified)
# ============================================================

# Create a new migration
Write-Host "`nCreating migration..." -ForegroundColor Green
dotnet ef migrations add [MigrationName] --project DAL --startup-project PL

# Apply migrations to database
Write-Host "`nApplying migrations..." -ForegroundColor Green
dotnet ef database update --project DAL --startup-project PL

# Remove last migration
Write-Host "`nRemoving last migration..." -ForegroundColor Yellow
dotnet ef migrations remove --project DAL --startup-project PL

# Script migration (generates SQL)
Write-Host "`nGenerating migration script..." -ForegroundColor Cyan
dotnet ef migrations script --output "migration.sql" --project DAL --startup-project PL

# ============================================================
# QUICK HEALTH CHECK SCRIPT
# ============================================================

Write-Host "`n`n" -ForegroundColor Cyan
Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║          NuGet Package Health Check                    ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green

# Step 1: Check SDK
Write-Host "`n[1/5] Checking .NET SDK..." -ForegroundColor Yellow
$sdkVersion = dotnet --version
if ($sdkVersion -match "10\.0") {
    Write-Host "✅ .NET SDK 10.x detected: $sdkVersion" -ForegroundColor Green
} else {
    Write-Host "❌ Unexpected SDK version: $sdkVersion" -ForegroundColor Red
}

# Step 2: Build solution
Write-Host "`n[2/5] Building solution..." -ForegroundColor Yellow
$buildResult = dotnet build 2>&1
if ($buildResult -match "Build succeeded") {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
}

# Step 3: Check EF Core
Write-Host "`n[3/5] Checking Entity Framework Core..." -ForegroundColor Yellow
$efVersion = dotnet ef --version 2>&1
if ($efVersion -match "10\.") {
    Write-Host "✅ EF Core 10.x detected" -ForegroundColor Green
} else {
    Write-Host "⚠️  EF Core version: $efVersion" -ForegroundColor Yellow
}

# Step 4: List migrations
Write-Host "`n[4/5] Checking migrations..." -ForegroundColor Yellow
$migrations = dotnet ef migrations list --project DAL --startup-project PL 2>&1
if ($migrations -match "MergeDatabasesIntegration") {
    Write-Host "✅ Merge migration found" -ForegroundColor Green
} else {
    Write-Host "⚠️  Merge migration not yet created" -ForegroundColor Yellow
}

# Step 5: Check database
Write-Host "`n[5/5] Database status..." -ForegroundColor Yellow
Write-Host "ℹ️  Use 'dotnet ef database update' to apply migrations" -ForegroundColor Cyan

Write-Host "`n╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                 Health Check Complete                   ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green

# ============================================================
# SUMMARY
# ============================================================

Write-Host @"

📦 QUICK COMMANDS SUMMARY
═══════════════════════════════════════════════════════════

To get started:
  1. Verify packages:  dotnet restore
  2. Build project:    dotnet build
  3. Apply migration:  dotnet ef database update --project DAL --startup-project PL

Common tasks:
  • List packages:     dotnet list package
  • Check outdated:    dotnet package update --dry-run
  • Clear cache:       dotnet nuget locals all --clear
  • Check EF Core:     dotnet ef --version

For more info, see: PACKAGE_VERIFICATION_REPORT.md
═══════════════════════════════════════════════════════════

"@ -ForegroundColor Cyan
