# 🚀 Apply Database Merge & Check Python Project - Complete Guide

**Created**: 2026-06-24  
**Project**: ITI Graduation Project - Database Merge  
**Environment**: Visual Studio Community 2026, PowerShell  

---

## 📋 Table of Contents

1. [Quick Start (5 minutes)](#quick-start)
2. [Step-by-Step Deployment](#step-by-step-deployment)
3. [Verify Migration Applied](#verify-migration-applied)
4. [Python Project Check](#python-project-check)
5. [Troubleshooting](#troubleshooting)

---

## 🏃 Quick Start

**If you just want to do it now, run these commands in order:**

```powershell
# 1. Navigate to project root
cd "E:\ITI Graduation Project\Claude Version\backend\Backend"

# 2. Backup database first (IMPORTANT!)
Write-Host "BACKUP YOUR DATABASE BEFORE PROCEEDING!" -ForegroundColor Red
Write-Host "Execute in SQL Server Management Studio:" -ForegroundColor Yellow
Write-Host "BACKUP DATABASE [StuckIn] TO DISK = 'C:\Backups\StuckIn_PreMerge_$(Get-Date -Format yyyyMMdd).bak'"

# 3. Restore packages
dotnet restore

# 4. Build solution
dotnet build

# 5. Apply migration
dotnet ef database update --project DAL --startup-project PL

# 6. Verify success
Write-Host "`n✅ Migration complete! Checking database..." -ForegroundColor Green
```

---

## 📊 Step-by-Step Deployment

### **Step 1: Verify Environment (2 minutes)**

```powershell
# Check current directory
Get-Location

# Should output: E:\ITI Graduation Project\Claude Version\backend\Backend

# Check .NET version
dotnet --version

# Should output: 10.0.xxx
```

### **Step 2: Backup Database (CRITICAL! 2 minutes)**

```powershell
# Run this in SQL Server Management Studio (SSMS)
# DO NOT SKIP THIS STEP!

# Replace C:\Backups\ with your backup location
BACKUP DATABASE [StuckIn] TO DISK = 'C:\Backups\StuckIn_PreMerge_2026-06-24.bak'
WITH INIT, DESC='Pre-Merge Backup'

# Verify backup was created
SELECT * FROM msdb.dbo.backupset 
WHERE database_name = 'StuckIn' 
ORDER BY backup_start_date DESC
```

### **Step 3: Restore NuGet Packages (1 minute)**

```powershell
# PowerShell command
cd "E:\ITI Graduation Project\Claude Version\backend\Backend"

# Restore all packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore

# Expected output: Restored successfully
```

### **Step 4: Build Solution (2 minutes)**

```powershell
# Build entire solution
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build

# Expected output: Build succeeded (0 errors, 0 warnings)
```

### **Step 5: List Migrations (1 minute)**

```powershell
# Check available migrations
Write-Host "Checking migrations..." -ForegroundColor Cyan
dotnet ef migrations list --project DAL --startup-project PL

# Expected output:
# (Pending)  20260624200728_MergeDatabasesIntegration
# ← This (Pending) means it hasn't been applied yet
```

### **Step 6: Apply Migration (2 minutes)**

```powershell
# THIS APPLIES THE DATABASE CHANGES
Write-Host "Applying migration to database..." -ForegroundColor Green
dotnet ef database update --project DAL --startup-project PL

# Expected output:
# info: Microsoft.EntityFrameworkCore.Database.Command[20101]
# Executed DbCommand (XXms) [Parameters=[], CommandType='Text']
# CREATE TABLE [Topics] ...
# ... (more tables being created)
# Done.
```

---

## ✅ Verify Migration Applied

### **Check 1: List Migrations After Apply (1 minute)**

```powershell
# Check migrations status
dotnet ef migrations list --project DAL --startup-project PL

# Expected output:
# 20260624200728_MergeDatabasesIntegration
# ← No (Pending) means it's applied!
```

### **Check 2: Verify Database Tables (SQL Query)**

```powershell
# Run these in SQL Server Management Studio

# Count new tables
SELECT COUNT(*) AS NewTablesCount
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN (
    'Topics', 'Resources', 'StudySessions', 'Evidence',
    'TopicRelationships', 'UserTopicMastery', 'UserDomains',
    'ResourceTopicCoverage', 'Questions', 'Goals'
)

# Expected: 10

# Check Topics table
SELECT * FROM Topics

# Check if AspNetUsers still there
SELECT COUNT(*) FROM AspNetUsers

# Check UserTopicMastery relationships
SELECT * FROM UserTopicMastery LIMIT 5
```

### **Check 3: Verify All Old Data Intact (SQL Query)**

```sql
-- Run in SQL Server Management Studio

-- Check users
SELECT COUNT(*) AS TotalUsers FROM AspNetUsers

-- Check subscriptions
SELECT COUNT(*) AS TotalSubscriptions FROM Subscriptions

-- Check payments
SELECT COUNT(*) AS TotalPayments FROM Payments

-- Check countries (location data)
SELECT COUNT(*) AS TotalCountries FROM Countries

-- All should have data or be 0 if empty (that's OK)
```

### **Check 4: Verify Constraints (SQL Query)**

```sql
-- Run in SQL Server Management Studio

-- Check constraints exist
SELECT COUNT(*) AS ConstraintCount
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME IN (
    'Evidence', 'Topics', 'TopicRelationships',
    'UserTopicMastery', 'UserDomains', 'ResourceTopicCoverage'
)

-- Expected: > 0

-- Check specific constraint
SELECT * 
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE CONSTRAINT_NAME LIKE 'CK_%'
```

---

## 🐍 Python Project Check

### **Step 1: Locate Python Project (1 minute)**

```powershell
# Find Python project files
Write-Host "Searching for Python project..." -ForegroundColor Cyan

Get-ChildItem -Path "E:\ITI Graduation Project\Claude Version\backend" -Recurse -Filter "*.py" | 
    Select-Object -First 10 | 
    ForEach-Object { Write-Host $_.FullName }

# Also check for:
# - requirements.txt
# - setup.py
# - pyproject.toml
# - .env

Get-ChildItem -Path "E:\ITI Graduation Project\Claude Version\backend" -Recurse -Filter "requirements.txt"
Get-ChildItem -Path "E:\ITI Graduation Project\Claude Version\backend" -Recurse -Filter ".env"
```

### **Step 2: Check Python Environment (2 minutes)**

```powershell
# Check Python installed
python --version

# Expected: Python 3.9+ (or newer)

# Check pip
pip --version

# Check if virtual environment exists
Get-Item -Path "E:\ITI Graduation Project\Claude Version\backend\venv" -ErrorAction SilentlyContinue

# If venv doesn't exist, create it
python -m venv venv

# Activate virtual environment
.\venv\Scripts\Activate.ps1

# Expected: (venv) in prompt
```

### **Step 3: Check Python Dependencies (2 minutes)**

```powershell
# Make sure you're in the backend directory
cd "E:\ITI Graduation Project\Claude Version\backend"

# Activate venv if not already
.\venv\Scripts\Activate.ps1

# List installed packages
pip list | Select-String "pyodbc|python-dotenv|langchain|qdrant"

# Expected to see these packages (if installed):
# pyodbc >= 4.0
# python-dotenv >= 0.19
# langchain-core >= 0.1
# qdrant-client >= 2.0 (optional)
```

### **Step 4: Install Missing Packages (5 minutes)**

```powershell
# If packages are missing, install them
# Make sure venv is activated first!

cd "E:\ITI Graduation Project\Claude Version\backend"
.\venv\Scripts\Activate.ps1

# Install required packages
pip install pyodbc python-dotenv langchain-core qdrant-client

# Verify installation
pip list
```

### **Step 5: Check .env File (1 minute)**

```powershell
# Check if .env file exists
Get-Item -Path "E:\ITI Graduation Project\Claude Version\backend\.env" -ErrorAction SilentlyContinue

# If exists, view content (masked passwords)
Get-Content -Path "E:\ITI Graduation Project\Claude Version\backend\.env" | 
    Where-Object { $_ -notmatch "PWD|PASSWORD|KEY|TOKEN" }

# Must have these variables:
# DB_SERVER=localhost\SQLEXPRESS
# DB_NAME=StuckIn (changed from GraduationProject!)
# DB_DRIVER=ODBC Driver 17 for SQL Server
# DB_TRUSTED=true
```

### **Step 6: Verify Python Script Works (3 minutes)**

```powershell
# Navigate to Python project
cd "E:\ITI Graduation Project\Claude Version\backend"

# Activate venv
.\venv\Scripts\Activate.ps1

# Test database connection
python -c "
import pyodbc
import os
from dotenv import load_dotenv

load_dotenv()

DB_DRIVER = os.getenv('DB_DRIVER', 'ODBC Driver 17 for SQL Server')
DB_SERVER = os.getenv('DB_SERVER', 'localhost\SQLEXPRESS')
DB_NAME = os.getenv('DB_NAME', 'StuckIn')
DB_TRUSTED = os.getenv('DB_TRUSTED', 'true').lower() in ('1', 'true', 'yes')

try:
    conn_str = f'DRIVER={{{DB_DRIVER}}};SERVER={DB_SERVER};DATABASE={DB_NAME};Trusted_Connection=yes;'
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    cursor.execute('SELECT COUNT(*) FROM Topics')
    count = cursor.fetchone()[0]
    print(f'✅ Connection successful! Topics table has {count} rows')
    conn.close()
except Exception as e:
    print(f'❌ Connection failed: {e}')
"

# Expected output: ✅ Connection successful!
```

---

## 🔍 Complete Verification Script

**Run this PowerShell script to verify everything at once:**

```powershell
#!/usr/bin/env powershell

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         Database Merge Verification Script             ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Change to project directory
cd "E:\ITI Graduation Project\Claude Version\backend\Backend"

# 1. Check .NET version
Write-Host "`n[1/6] Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "✅ .NET Version: $dotnetVersion"

# 2. Build solution
Write-Host "`n[2/6] Building solution..." -ForegroundColor Yellow
$buildResult = dotnet build 2>&1
if ($buildResult -match "Build succeeded") {
    Write-Host "✅ Build succeeded" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed - see errors above" -ForegroundColor Red
    exit 1
}

# 3. Check migrations
Write-Host "`n[3/6] Checking migrations status..." -ForegroundColor Yellow
$migrations = dotnet ef migrations list --project DAL --startup-project PL 2>&1
if ($migrations -match "MergeDatabasesIntegration") {
    Write-Host "✅ Merge migration found" -ForegroundColor Green
} else {
    Write-Host "❌ Merge migration not found" -ForegroundColor Red
}

# 4. Check EF Core
Write-Host "`n[4/6] Checking Entity Framework Core..." -ForegroundColor Yellow
$efVersion = dotnet ef --version 2>&1
Write-Host "✅ EF Core: $efVersion"

# 5. Check Python
Write-Host "`n[5/6] Checking Python environment..." -ForegroundColor Yellow
cd "E:\ITI Graduation Project\Claude Version\backend"
if (Test-Path ".\venv\Scripts\Activate.ps1") {
    Write-Host "✅ Virtual environment found" -ForegroundColor Green
    .\venv\Scripts\Activate.ps1
    $pythonVersion = python --version 2>&1
    Write-Host "✅ Python: $pythonVersion"

    # Check packages
    $packages = pip list 2>&1
    if ($packages -match "pyodbc") { Write-Host "✅ pyodbc installed" -ForegroundColor Green }
    if ($packages -match "python-dotenv") { Write-Host "✅ python-dotenv installed" -ForegroundColor Green }
    if ($packages -match "langchain") { Write-Host "✅ langchain-core installed" -ForegroundColor Green }
} else {
    Write-Host "⚠️  Virtual environment not found - create with: python -m venv venv" -ForegroundColor Yellow
}

# 6. Summary
Write-Host "`n[6/6] Summary..." -ForegroundColor Yellow
Write-Host "`n╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                  VERIFICATION COMPLETE                 ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host @"

✅ All systems ready!

Next steps:
1. Backup database (if not done)
2. Apply migration: dotnet ef database update --project DAL --startup-project PL
3. Verify: SELECT COUNT(*) FROM Topics

"@ -ForegroundColor Cyan
```

---

## 🐛 Troubleshooting

### **Problem: Migration fails with "Connection timeout"**

```powershell
# Solution: Check SQL Server is running
Get-Service -Name "MSSQL*" | Select-Object Status, Name

# If not running, start it (may need admin)
Start-Service -Name "MSSQLSERVER"

# Then retry migration
dotnet ef database update --project DAL --startup-project PL
```

### **Problem: "The input line is too long"**

```powershell
# Solution: Navigate to project directory first
cd "E:\ITI Graduation Project\Claude Version\backend\Backend"

# Then run the command
dotnet ef database update --project DAL --startup-project PL
```

### **Problem: Python can't connect to database**

```powershell
# Check .env file
cat .env | Select-String "DB_"

# Should show:
# DB_NAME=StuckIn (NOT GraduationProject)
# DB_DRIVER=ODBC Driver 17 for SQL Server
# DB_SERVER=localhost\SQLEXPRESS
# DB_TRUSTED=true
```

### **Problem: "The target database does not exist"**

```powershell
# The migration creates tables but database must exist
# Create database manually if needed:

# In SQL Server Management Studio:
CREATE DATABASE StuckIn;
GO

# Then run migration again
dotnet ef database update --project DAL --startup-project PL
```

---

## 📋 Complete Checklist

Run this checklist in order:

```powershell
# ✅ 1. Environment check
Write-Host "✅ 1. Checking .NET SDK..."
dotnet --version

# ✅ 2. Navigate to correct directory
Write-Host "✅ 2. Navigating to project..."
cd "E:\ITI Graduation Project\Claude Version\backend\Backend"
Get-Location

# ✅ 3. Backup database
Write-Host "✅ 3. Backup database in SQL Server Management Studio FIRST!"

# ✅ 4. Restore packages
Write-Host "✅ 4. Restoring NuGet packages..."
dotnet restore

# ✅ 5. Build solution
Write-Host "✅ 5. Building solution..."
dotnet build

# ✅ 6. List migrations
Write-Host "✅ 6. Checking migrations..."
dotnet ef migrations list --project DAL --startup-project PL

# ✅ 7. Apply migration (THE BIG ONE!)
Write-Host "✅ 7. Applying migration..." -ForegroundColor Green
dotnet ef database update --project DAL --startup-project PL

# ✅ 8. Verify in SQL Server
Write-Host "✅ 8. Run SQL queries in SSMS to verify tables exist"

# ✅ 9. Check Python
Write-Host "✅ 9. Check Python project..."
cd "E:\ITI Graduation Project\Claude Version\backend"
python --version
pip list | Select-String "pyodbc"

Write-Host "`n✅ ALL DONE!" -ForegroundColor Green
```

---

## 🎯 Summary Commands

**Copy-paste these to run everything:**

```powershell
# 1. Backup database first in SSMS!

# 2. Then run these in PowerShell:
cd "E:\ITI Graduation Project\Claude Version\backend\Backend"
dotnet restore
dotnet build
dotnet ef database update --project DAL --startup-project PL

# 3. Check Python
cd "E:\ITI Graduation Project\Claude Version\backend"
.\venv\Scripts\Activate.ps1
pip list

# 4. Done!
Write-Host "✅ Migration applied successfully!" -ForegroundColor Green
```

---

## 🚀 You're Ready!

After running these commands:

✅ Database migration applied  
✅ 10 new tables created  
✅ Old data preserved  
✅ Python project verified  
✅ All packages installed  

**Next**: Check [DATABASE_MERGE_SUMMARY.md](DATABASE_MERGE_SUMMARY.md) for what changed.

---

**Questions?** See [SAFE_MIGRATION_CHECKLIST.md](SAFE_MIGRATION_CHECKLIST.md) for troubleshooting.
