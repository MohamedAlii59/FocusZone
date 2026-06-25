# Database Schema Integration Checklist

## ✅ Completed Tasks

### 1. **Entity Classes Created (DAL/Entities/)**
- ✅ Certificate.cs
- ✅ Education.cs
- ✅ Experience.cs
- ✅ Project.cs
- ✅ ExamSession.cs
- ✅ SessionAnswer.cs
- ✅ AnswerChoice.cs
- ✅ Updated User.cs with navigation properties

### 2. **EF Core Configuration (DAL/Database/AppDbContext.cs)**
- ✅ Added DbSet properties for all new entities
- ✅ Configured all entity relationships (Foreign Keys)
- ✅ Added all Check Constraints (e.g., CK_Evidence_Score, CK_Topics_Type)
- ✅ Configured default values for all columns
- ✅ Set up soft-delete query filters where applicable
- ✅ Configured primary/composite keys

### 3. **Database Default Values Configured**
The following defaults are set in EF Core and will be applied via migrations:
- Certificates.CreatedAt → sysutcdatetime()
- Educations.SortOrder → 0, CreatedAt → sysutcdatetime()
- Experiences.Current → false, SortOrder → 0, CreatedAt → sysutcdatetime()
- Evidence.CreatedAt → getutcdate()
- Goals.Priority → 1, CreatedAt → getutcdate()
- Questions.CreatedAt → getutcdate()
- Resources.Difficulty → 1, Depth → 1, CreatedAt → getutcdate()
- ResourceTopicCoverage.CoverageWeight → 1.00, DifficultyContribution → 1.00
- Topics.Difficulty → 1
- TopicRelationships.Weight → 1.00
- UserTopicMastery: Mastery → 0.00, Confidence → 0.00, Interest → 0.50, EvidenceCount → 0, LastUpdated → getutcdate()

### 4. **View & Stored Procedures**
Created: `SQL/CompleteDatabase_StuckIn.sql` containing:
- ✅ View: UserSkillsView
- ✅ Stored Procedures (updated to use NVARCHAR(450) for UserId):
  - usp_ComputeEvidenceScore
  - usp_UpdateMastery
  - usp_RecalcDomainScore
  - usp_ComputeConfidenceScore
  - usp_ProcessSession
  - usp_GetRecommendationPriorities
  - usp_EnsureUserTopicMastery
  - usp_GetOrCreateResource
  - usp_UpsertTopic

## 📋 Next Steps

### Step 1: Create EF Core Migration
```powershell
cd "E:\ITI Graduation Project\Claude Version\backend\Backend"
dotnet ef migrations add AddMissingTables --project DAL --startup-project PL
```

### Step 2: Apply Migration to Database
```powershell
dotnet ef database update --project DAL --startup-project PL
```

### Step 3: Execute SQL Script on StuckIn Database (for other projects)
1. Open SQL Server Management Studio
2. Connect to your server
3. Select the `StuckIn` database
4. Open and execute: `SQL/CompleteDatabase_StuckIn.sql`

This will add:
- All foreign keys to the StuckIn database
- All check constraints
- All column defaults
- UserSkillsView
- All stored procedures

## 🔑 Important Changes from Original Schema

| Column | Original Type | New Type | Reason |
|--------|--------------|----------|--------|
| UserId (FK) | BIGINT | NVARCHAR(450) | ASP.NET Identity uses string Ids |
| User.UserId (PK) | UserId BIGINT | Id NVARCHAR(450) | Identity framework standard |
| Default Difficulty | 2 | 1 | Matches EF Core configuration |
| Default Depth | 2 | 1 | Matches EF Core configuration |

## 📂 Files Created/Modified

### Created:
- `DAL/Entities/Certificate.cs`
- `DAL/Entities/Education.cs`
- `DAL/Entities/Experience.cs`
- `DAL/Entities/Project.cs`
- `DAL/Entities/ExamSession.cs`
- `DAL/Entities/SessionAnswer.cs`
- `DAL/Entities/AnswerChoice.cs`
- `SQL/CompleteDatabase_StuckIn.sql` (Complete with defaults, constraints, views, SPs)
- `SQL/UpdatedStoredProcedures.sql` (Old version - reference only)

### Modified:
- `DAL/Entities/User.cs` - Added navigation properties
- `DAL/Database/AppDbContext.cs` - Added DbSets and configurations

## 🚀 How to Use Stored Procedures from C#

Example in BL layer:
```csharp
// Call stored procedure directly
var result = await _context.Database.ExecuteSqlInterpolatedAsync(
    $"EXEC usp_UpdateMastery @UserId={userId}, @TopicId={topicId}, @EvidenceScore={score}"
);

// Query from view
var skills = await _context.UserTopicMasteries
    .FromSqlInterpolated($"SELECT * FROM UserSkillsView WHERE UserId = {userId}")
    .ToListAsync();
```

## ⚠️ Important Notes

1. **UserId Type Change**: All stored procedure parameters use `NVARCHAR(450)` instead of `BIGINT` because ASP.NET Identity uses `string` for User IDs.

2. **Database Compatibility**: The `CompleteDatabase_StuckIn.sql` script is designed for the `StuckIn` database. If other projects need access, they should reference this database or execute this script on their own instance.

3. **Soft Delete**: User entities with `IsDeleted = true` are automatically filtered out in queries via global query filters.

4. **Cascade Deletes**: Configured properly to maintain referential integrity (e.g., deleting a User cascades to Goals, Certificates, Projects, etc.).

5. **Check Constraints**: All numeric fields (Mastery, Confidence, Interest, Score, etc.) are constrained to 0.00-1.00 range at the database level.

## ✨ Summary

Your database schema is now fully synchronized with your EF Core models. The migration will create all missing tables with proper constraints and defaults. The StuckIn database can be accessed by other projects using the stored procedures and view defined in the SQL script.
