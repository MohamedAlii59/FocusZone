# Database Merge Implementation Guide

## Overview
This document explains the successful merge of two databases:
- **AspNetDatabase** (authentication, subscriptions, payments)
- **GraduationProject Database** (topics, resources, study sessions, evidence, mastery tracking)

The merged database is now unified into a single **StuckIn** database while maintaining all relationships and data integrity.

---

## Architecture Changes

### What Changed

#### 1. **Entity Model Expansion**
Added new entities to support graduation project features:
- `Topic` - Learning topics/skills with types (Domain, Concept, Technique, Tool, Career)
- `Resource` - Learning materials (YouTube, courses, books, articles, etc.)
- `StudySession` - User study sessions linked to resources
- `Evidence` - Learning evidence (quiz scores, study time, assessments)
- `TopicRelationship` - Relationships between topics (contains, prerequisite_for, related_to, required_for)
- `UserTopicMastery` - User mastery levels for each topic
- `UserDomain` - User domain-level scores
- `ResourceTopicCoverage` - Topics covered by each resource
- `Question` - Questions asked during sessions
- `Goal` - Merged from old Goals and UserInterests tables

#### 2. **Foreign Key References**
All new entities use `UserId` (string) from `AspNetUsers` table:
```csharp
StudySession.UserId → AspNetUsers.Id
UserTopicMastery.UserId → AspNetUsers.Id
UserDomain.UserId → AspNetUsers.Id
Goal.UserId → AspNetUsers.Id
Question.UserId → AspNetUsers.Id
```

#### 3. **Database Tables Created**
| Table | Purpose |
|-------|---------|
| `Topics` | Store learning topics with hierarchy |
| `Resources` | Store learning materials |
| `StudySessions` | Track user study sessions |
| `Evidence` | Store learning evidence (quiz/assessment scores) |
| `TopicRelationships` | Define relationships between topics |
| `UserTopicMasteries` | Track user mastery for each topic |
| `UserDomains` | Track user performance in domain areas |
| `ResourceTopicCoverages` | Map resources to topics they cover |
| `Questions` | Store questions from sessions |
| `Goals` | Unified goals and interests |

---

## Migration Steps

### Step 1: Apply EF Core Migration
```bash
cd Backend
dotnet ef database update --project DAL --startup-project PL
```

This creates all new tables in the `StuckIn` database while preserving existing data.

### Step 2: Register Repositories & Utilities (Startup Configuration)

In your `Program.cs` or `Startup.cs`:

```csharp
// Add the graduation project repository
services.AddScoped<GraduationProjectRepository>();

// Add migration utility for data import (optional)
services.AddScoped<DataMigrationUtility>();
```

### Step 3: Data Migration from Old Database (if needed)

If you have existing data in the old GraduationProject database, use the migration utility:

```csharp
var migrationUtility = new DataMigrationUtility(dbContext);

// Migrate your data
await migrationUtility.MigrateTopicsAsync(oldTopicsData);
await migrationUtility.MigrateResourcesAsync(oldResourcesData);
await migrationUtility.MigrateStudySessionsAsync(oldSessionsData);
await migrationUtility.MigrateEvidenceAsync(oldEvidenceData);

// Verify migration success
var report = await migrationUtility.VerifyMigrationAsync();
Console.WriteLine(report.ToString());
```

---

## API Usage Examples

### Using the Repository Layer

```csharp
// Inject the repository
[ApiController]
[Route("api/[controller]")]
public class LearningController : ControllerBase
{
    private readonly GraduationProjectRepository _repo;

    public LearningController(GraduationProjectRepository repo)
    {
        _repo = repo;
    }

    // Create a study session
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession(string userId, long resourceId)
    {
        var session = await _repo.CreateStudySessionAsync(
            userId, 
            resourceId, 
            "Study session summary", 
            60.0f // duration in minutes
        );
        return Ok(session);
    }

    // Get user mastery data
    [HttpGet("users/{userId}/mastery")]
    public async Task<IActionResult> GetUserMastery(string userId)
    {
        var masteries = await _repo.GetUserMasteriesAsync(userId);
        return Ok(masteries);
    }

    // Create evidence (quiz/assessment results)
    [HttpPost("evidence")]
    public async Task<IActionResult> RecordEvidence(long sessionId, int topicId, decimal score)
    {
        var evidence = await _repo.CreateEvidenceAsync(
            sessionId, 
            topicId, 
            "quiz", 
            score
        );
        return Ok(evidence);
    }
}
```

---

## Python Integration Updates

### Updated Connection String
```python
import os
from dotenv import load_dotenv

load_dotenv()

DB_SERVER = os.getenv("DB_SERVER", "localhost\\SQLEXPRESS")
DB_NAME = os.getenv("DB_NAME", "StuckIn")  # Now unified database
DB_TRUSTED = os.getenv("DB_TRUSTED", "true").lower() in ("1", "true", "yes")

# Build connection string
if DB_TRUSTED:
    auth_part = "Trusted_Connection=yes;"
else:
    DB_UID = os.getenv("DB_UID", "")
    DB_PWD = os.getenv("DB_PWD", "")
    auth_part = f"UID={DB_UID};PWD={DB_PWD};"

DB_CONN_STR = f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={DB_SERVER};DATABASE={DB_NAME};{auth_part}"
```

### Key Changes in database_tools.py
1. **UserId is now a string** (changed from int in old system):
   ```python
   def create_study_session(user_id: str, ...) -> dict:  # Changed from int
   ```

2. **References AspNetUsers table**:
   ```python
   def fetch_user_profile(user_id: str) -> Dict[str, str]:
       cur.execute("SELECT UserName, Email FROM AspNetUsers WHERE Id = ?", user_id)
   ```

3. **Goals table now unified**:
   - Replaced separate UserInterests and Goals tables
   - Use `Category` column to distinguish types
   - Query all with: `SELECT * FROM Goals WHERE UserId = ?`

4. **Stored procedures still available**:
   - `usp_ProcessSession` - Update mastery and confidence
   - `usp_ComputeEvidenceScore` - Compute weighted evidence score
   - `usp_ComputeConfidenceScore` - Calculate confidence metric
   - `usp_RecalcDomainScore` - Recalculate domain-level scores
   - `usp_UpdateMastery` - Update user mastery (EMA)
   - `usp_UpsertTopic` - Create or get topic
   - `usp_GetOrCreateResource` - Create or get resource

---

## Database Views

### UserSkillsView
Simplified view for fetching user skills:
```sql
SELECT
    UserId,
    TopicId,
    SkillName (Topic.Name),
    Proficiency (UserTopicMastery.Mastery),
    Confidence,
    EvidenceCount,
    LastUpdated
FROM UserTopicMastery
JOIN Topics ON UserTopicMastery.TopicId = Topics.TopicId
```

**Usage in Python:**
```python
def fetch_skills_from_view(user_id: str, limit: int = 50) -> List[Dict[str, Any]]:
    cur.execute(
        "SELECT SkillName, Proficiency, Confidence FROM UserSkillsView WHERE UserId = ?",
        user_id
    )
    return [{"name": r[0], "proficiency": float(r[1]), "confidence": float(r[2])} for r in cur.fetchall()]
```

---

## Data Integrity Constraints

All merged tables include check constraints for data validity:

```sql
-- Evidence scores must be 0.00 - 1.00
CK_Evidence_Score: Score >= 0.00 AND Score <= 1.00

-- Evidence types are restricted
CK_Evidence_Type: Type IN ('quiz', 'study_time', 'assessment', 'retention_test')

-- Topic types are restricted
CK_Topics_Type: Type IN ('Domain', 'Concept', 'Technique', 'Tool', 'Career')

-- Topic relationships prevent self-references
CK_TopicRel_NoSelfRef: SourceTopicId <> TargetTopicId

-- Mastery/Confidence/Interest scores are bounded
CK_UTM_Mastery: Mastery >= 0.00 AND Mastery <= 1.00
CK_UTM_Confidence: Confidence >= 0.00 AND Confidence <= 1.00
CK_UTM_Interest: Interest >= 0.00 AND Interest <= 1.00
```

---

## Qdrant Integration (Vector Database)

The Qdrant setup remains unchanged. The `02_qdrant_setup.py` continues to use collections:
- `ResourceEmbeddings` - Resource vector embeddings
- `SessionEmbeddings` - Study session embeddings
- `TopicEmbeddings` - Topic vector embeddings
- `SessionChatHistory` - Conversation history

**No schema changes needed in Qdrant** - it operates independently of the SQL database structure.

---

## Backward Compatibility

### What Still Works
✅ All existing AspNetUsers data and authentication
✅ Subscriptions and Payments tables
✅ All EF Core DbSet operations
✅ Identity framework integration
✅ Soft delete functionality (IsDeleted flag on User)

### What Changed
⚠️ UserId type in new entities (now string instead of int)
⚠️ Goals/Interests now in unified Goals table
⚠️ Need to use new repository for graduation project features

### Migration Path
For existing .NET code referencing only AspNetDatabase:
- ✅ No changes needed - all old tables still exist
- Add references to new entities as needed
- Use `GraduationProjectRepository` for new features

---

## Troubleshooting

### Issue: Migration fails due to foreign key constraints
**Solution**: Ensure all Users exist before importing study sessions
```csharp
// Check if user exists first
var user = await context.Users.FindAsync(userId);
if (user == null) {
    throw new InvalidOperationException($"User {userId} does not exist");
}
```

### Issue: Qdrant updates not syncing
**Solution**: Check Qdrant connection in database_tools.py
```python
if QDRANT_AVAILABLE:
    try:
        upsert_session(...)
    except Exception as e:
        print(f"Qdrant sync failed (non-fatal): {e}")
```

### Issue: Python connection string not working
**Solution**: Verify .env file has correct database name:
```
DB_NAME=StuckIn
DB_DRIVER=ODBC Driver 17 for SQL Server
DB_SERVER=localhost\SQLEXPRESS
DB_TRUSTED=true
```

---

## Testing the Merge

### Basic Integration Test
```csharp
[Test]
public async Task TestMergedDatabaseIntegration()
{
    var user = new User { Id = "test-user-1", UserName = "testuser" };
    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    // Create topic
    var topic = await _repo.GetOrCreateTopicAsync("C# Basics");
    Assert.IsNotNull(topic);

    // Create resource
    var resource = await _repo.GetOrCreateResourceAsync(
        "https://example.com/resource",
        "C# Tutorial",
        60.0f
    );
    Assert.IsNotNull(resource);

    // Create session
    var session = await _repo.CreateStudySessionAsync(
        user.Id,
        resource.ResourceId,
        "Learned basics",
        60.0f
    );
    Assert.IsNotNull(session);

    // Create evidence
    var evidence = await _repo.CreateEvidenceAsync(
        session.SessionId,
        topic.TopicId,
        "quiz",
        0.85m
    );
    Assert.AreEqual(0.85m, evidence.Score);
}
```

---

## Summary

✅ **Two databases merged successfully into one**
✅ **All relationships intact with proper foreign keys**
✅ **AspNetUsers is the central reference point**
✅ **New repository layer for easy access**
✅ **EF Core migrations handle schema updates**
✅ **Python tools updated for new connection**
✅ **Data integrity constraints enforced**
✅ **Backward compatibility maintained**

Your system is now unified! 🎉
