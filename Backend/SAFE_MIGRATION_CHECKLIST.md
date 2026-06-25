# ⚠️ SAFE DATABASE MERGE - CRITICAL CHECKLIST

## Overview
This checklist ensures the database merge doesn't break anything. Follow these steps carefully.

---

## Pre-Migration Checklist

### ✅ Backup Existing Database
```bash
# Backup the original databases before any changes
# 1. Backup AspNetDatabase (current)
# 2. Backup GraduationProject (source)
# 3. Keep these backups for 30 days minimum
```

### ✅ Review Connection Strings
- [ ] Current `appsettings.json` uses correct server and database name
- [ ] `.env` file has correct values for DB_SERVER, DB_NAME, DB_TRUSTED
- [ ] Python scripts will work with new merged database

### ✅ Check Entity Relationships
- [ ] All User references in old code use `IdentityUser.Id` (string)
- [ ] No hardcoded integer user IDs in queries
- [ ] Foreign key constraints won't fail during migration

---

## Migration Steps (DO NOT SKIP)

### Step 1: Backup and Review
```bash
# Create backups FIRST
BACKUP DATABASE [StuckIn] TO DISK = 'C:\Backups\StuckIn_PreMerge_$(date).bak'
```

### Step 2: Review Existing Data
```bash
# Check for any data that might conflict
SELECT COUNT(*) FROM AspNetUsers
SELECT COUNT(*) FROM Resources  -- Should be 0 if new
SELECT COUNT(*) FROM StudySessions  -- Should be 0 if new
```

### Step 3: Run EF Migrations
```bash
cd "E:\ITI Graduation Project\Claude Version\backend\Backend"
dotnet ef migrations list --project DAL --startup-project PL
# Should see: MergeDatabasesIntegration

dotnet ef database update --project DAL --startup-project PL
# Wait for completion - DO NOT INTERRUPT
```

### Step 4: Verify Schema
```bash
# Verify new tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Topics', 'Resources', 'StudySessions', 'Evidence', 'Goals')
```

### Step 5: Test Connection
```bash
# Test with simple query
SELECT COUNT(*) FROM AspNetUsers  -- Old data still there
SELECT COUNT(*) FROM Topics  -- New table, should be empty or have seeds
```

---

## What NOT To Do

### ❌ DON'T
- ❌ Run migration without backup
- ❌ Skip the `dotnet ef migrations list` verification
- ❌ Use hardcoded int user IDs in Python code
- ❌ Manually modify migrated schema without EF
- ❌ Share backups over unsecured channels
- ❌ Run migrations on production without testing staging first
- ❌ Ignore warning messages in migration output

### ✅ DO
- ✅ Test in development environment first
- ✅ Keep migration scripts for audit trail
- ✅ Verify each step completes successfully
- ✅ Run full test suite after migration
- ✅ Update documentation with actual server details
- ✅ Inform team when migration is complete

---

## Backward Compatibility Verification

### ✅ Old Functionality Still Works
```csharp
// Test 1: Users table still works
var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
Assert.NotNull(user);

// Test 2: Authentication still works
var signInResult = await signInManager.PasswordSignInAsync(
    user.UserName, "password", false, false);
Assert.True(signInResult.Succeeded);

// Test 3: Subscriptions still work
var subscription = await context.Subscriptions
    .FirstOrDefaultAsync(s => s.UserId == user.Id);
Assert.NotNull(subscription);

// Test 4: Payments still work
var payment = await context.Payments
    .FirstOrDefaultAsync(p => p.UserId == user.Id);
Assert.NotNull(payment);
```

### ✅ New Functionality Works
```csharp
var repo = new GraduationProjectRepository(context);

// Test 1: Create topic
var topic = await repo.GetOrCreateTopicAsync("C# Basics");
Assert.NotNull(topic);
Assert.IsTrue(topic.TopicId > 0);

// Test 2: Create resource
var resource = await repo.GetOrCreateResourceAsync(
    "https://example.com",
    "Tutorial",
    60.0f
);
Assert.NotNull(resource);

// Test 3: Create study session
var session = await repo.CreateStudySessionAsync(
    user.Id,  // Now uses string UserId
    resource.ResourceId,
    "Summary",
    60.0f
);
Assert.NotNull(session);

// Test 4: Create evidence
var evidence = await repo.CreateEvidenceAsync(
    session.SessionId,
    topic.TopicId,
    "quiz",
    0.85m
);
Assert.AreEqual(0.85m, evidence.Score);
```

---

## Rollback Plan

If something goes wrong, follow this procedure:

### Option 1: Quick Rollback (if migration failed)
```bash
# Undo the last migration
dotnet ef migrations remove --project DAL --startup-project PL

# Restore from backup
RESTORE DATABASE [StuckIn] FROM DISK = 'C:\Backups\StuckIn_PreMerge.bak'
```

### Option 2: Full Rollback (if major issues)
```bash
# 1. Stop the application
# 2. Restore from backup
RESTORE DATABASE [StuckIn] FROM DISK = 'C:\Backups\StuckIn_PreMerge.bak'

# 3. Restore code to previous state
git checkout HEAD~1

# 4. Rebuild solution
dotnet clean
dotnet build

# 5. Notify team and investigate
```

### Option 3: Partial Rollback (migrate subset)
```bash
# If only specific feature has issues, you can:
# 1. Comment out problematic DbSet in AppDbContext
# 2. Remove those entities from migration
# 3. Re-run: dotnet ef migrations remove
# 4. Fix issue
# 5. Re-create migration
```

---

## Breaking Changes to Watch For

### 1️⃣ UserId Type Changed
```csharp
// ❌ WRONG - Old way
var userId = 123; // int
var session = await context.StudySessions
    .FirstOrDefaultAsync(s => s.UserId == userId); // TYPE ERROR

// ✅ CORRECT - New way
var userId = "user-guid-string"; // string from AspNetUsers.Id
var session = await context.StudySessions
    .FirstOrDefaultAsync(s => s.UserId == userId);
```

### 2️⃣ Goals/Interests Merged
```csharp
// ❌ WRONG - Old way
var interests = await context.UserInterests
    .Where(ui => ui.UserId == userId)
    .ToListAsync(); // Table doesn't exist anymore

// ✅ CORRECT - New way
var goals = await context.Goals
    .Where(g => g.UserId == userId && g.Category == "Interest")
    .ToListAsync();

var actualGoals = await context.Goals
    .Where(g => g.UserId == userId && g.Category == "Goal")
    .ToListAsync();
```

### 3️⃣ Python Database Connection
```python
# ❌ WRONG - Old way
DB_NAME = "GraduationProject"  # Old separate database

# ✅ CORRECT - New way
DB_NAME = "StuckIn"  # Unified database
```

### 4️⃣ Python User ID
```python
# ❌ WRONG - Old way
def create_study_session(user_id: int, ...):  # int

# ✅ CORRECT - New way
def create_study_session(user_id: str, ...):  # string from AspNetUsers.Id
    # user_id format: "user-guid-string"
```

---

## Testing After Migration

### Run These Tests in Order
```bash
# 1. Unit Tests - Data Access Layer
dotnet test DAL/DAL.csproj

# 2. Integration Tests - Database Operations
dotnet test DAL.Tests/DAL.Tests.csproj

# 3. API Tests - Full Stack
dotnet test PL.Tests/PL.Tests.csproj

# 4. Load Testing (optional)
# Run with production-like data volumes
```

### Manual Smoke Tests
- [ ] Login/Logout works
- [ ] Create new user works
- [ ] Subscribe to plan works
- [ ] Process payment works
- [ ] View dashboard works
- [ ] Create study session works (NEW)
- [ ] Record evidence works (NEW)
- [ ] View mastery data works (NEW)

---

## Monitoring After Migration

### Performance Metrics to Track
```sql
-- Check for slow queries
SELECT * FROM sys.dm_exec_query_stats
ORDER BY total_elapsed_time DESC

-- Index usage
SELECT * FROM sys.dm_db_index_usage_stats
WHERE database_id = DB_ID('StuckIn')
```

### Application Monitoring
- [ ] Monitor error logs for foreign key violations
- [ ] Check for N+1 queries in new endpoints
- [ ] Monitor database connection pool
- [ ] Track application startup time
- [ ] Monitor API response times

### Data Integrity Checks
```sql
-- Orphaned Evidence (session deleted but evidence exists)
SELECT COUNT(*) FROM Evidence e
WHERE NOT EXISTS (SELECT 1 FROM StudySessions ss WHERE ss.SessionId = e.SessionId)

-- Orphaned Sessions (user deleted but session exists)
SELECT COUNT(*) FROM StudySessions ss
WHERE NOT EXISTS (SELECT 1 FROM AspNetUsers au WHERE au.Id = ss.UserId)

-- Invalid Topic References
SELECT COUNT(*) FROM UserTopicMastery utm
WHERE NOT EXISTS (SELECT 1 FROM Topics t WHERE t.TopicId = utm.TopicId)
```

---

## Migration Sign-Off Checklist

Before declaring the migration complete:

- [ ] Backup created and verified
- [ ] Schema matches expected tables
- [ ] All foreign keys intact
- [ ] Old data still accessible
- [ ] New tables empty (ready for data)
- [ ] No migration warnings ignored
- [ ] Backward compatibility tests pass
- [ ] New feature tests pass
- [ ] Performance metrics acceptable
- [ ] Data integrity checks pass
- [ ] Documentation updated
- [ ] Team notified
- [ ] Rollback plan documented

---

## Common Issues & Solutions

### Issue 1: Migration Hangs
**Symptoms**: `dotnet ef database update` doesn't complete
**Solution**:
```bash
# Check for locks
SELECT * FROM sys.dm_tran_locks

# Kill blocking sessions if needed
KILL <session_id>

# Retry migration
dotnet ef database update --project DAL --startup-project PL
```

### Issue 2: Foreign Key Violation
**Symptoms**: "The INSERT, UPDATE, or DELETE statement conflicted with a FOREIGN KEY constraint"
**Solution**:
```bash
# Check which FK is violated
SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS

# Temporarily disable constraint for investigation
ALTER TABLE StudySessions NOCHECK CONSTRAINT FK_Sessions_User

# Investigate data
SELECT * FROM StudySessions WHERE UserId IS NULL

# Re-enable
ALTER TABLE StudySessions WITH CHECK CHECK CONSTRAINT FK_Sessions_User
```

### Issue 3: Duplicate Key in Unique Constraint
**Symptoms**: "Cannot insert duplicate key row in object"
**Solution**:
```bash
# Find duplicates
SELECT TopicId, Name, COUNT(*) 
FROM Topics 
GROUP BY TopicId, Name 
HAVING COUNT(*) > 1

# Delete duplicates (keep latest)
DELETE FROM Topics WHERE TopicId NOT IN (
    SELECT MAX(TopicId) FROM Topics GROUP BY Name
)
```

### Issue 4: Timeout During Migration
**Symptoms**: Operation timeout after 30 seconds
**Solution**:
```bash
# Increase timeout in AppDbContext
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.CommandTimeout(300); // 5 minutes
});

# Retry migration
dotnet ef database update --project DAL --startup-project PL
```

---

## Success Indicators ✅

You'll know the migration was successful when:

1. ✅ All tables exist in database
2. ✅ No schema mismatch warnings
3. ✅ Old user data is intact
4. ✅ Authentication works
5. ✅ New features are accessible
6. ✅ No foreign key errors
7. ✅ Performance is acceptable
8. ✅ Application starts without errors
9. ✅ Tests pass (old and new)
10. ✅ Team confirms functionality

---

## Next Steps

1. **Schedule Migration Window**
   - Best: Off-peak hours or weekend
   - Duration: 30 minutes to 2 hours
   - Have rollback plan ready

2. **Prepare Team**
   - Brief team on changes
   - Ensure Python developers update connection strings
   - Prepare support for issues

3. **Document Changes**
   - Update README.md
   - Update API documentation
   - Update deployment scripts

4. **Plan Migration Data**
   - Schedule import from old database
   - Prepare data mapping
   - Test import process first

---

## Support

If you encounter issues:
1. Check this document first
2. Review error messages in build output
3. Check backup is recent
4. Review git history for changes
5. Contact team lead

**Remember**: It's better to rollback and try again than to force through an issue! 🚀
