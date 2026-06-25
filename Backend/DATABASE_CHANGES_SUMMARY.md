# Database Schema & Stored Procedures - Summary of Changes

## Key Changes Made:

### 1. **UserId Parameter Type Changed**
- **OLD**: `@UserId BIGINT` 
- **NEW**: `@UserId NVARCHAR(450)` (ASP.NET Identity string)
- **Applied to**: All procedures that accept UserId:
  - `usp_UpdateMastery`
  - `usp_RecalcDomainScore`
  - `usp_ComputeConfidenceScore`
  - `usp_ProcessSession`
  - `usp_GetRecommendationPriorities`
  - `usp_EnsureUserTopicMastery`

### 2. **Table Names & Columns Verified**
All procedures now use correct table and column names from EF Core entities:

| Table | Key Columns |
|-------|------------|
| `Evidence` | EvidenceId (BIGINT), SessionId (BIGINT), TopicId (INT), Type (VARCHAR), Score (DECIMAL) |
| `StudySessions` | SessionId (BIGINT), UserId (NVARCHAR(450)), ResourceId (BIGINT), StartedAt, EndedAt |
| `UserTopicMastery` | UserId (NVARCHAR(450)), TopicId (INT), Mastery, Confidence, Interest, EvidenceCount, LastUpdated |
| `Topics` | TopicId (INT), Name (NVARCHAR), Type (VARCHAR), Difficulty (INT), EstimatedHours (DECIMAL) |
| `Resources` | ResourceId (BIGINT), Title (NVARCHAR), Type (VARCHAR), Url (VARCHAR), Difficulty, Depth, EstimatedMinutes, CreatedAt |
| `TopicRelationships` | RelationshipId (INT), SourceTopicId, TargetTopicId, RelationshipType, Weight (DECIMAL) |
| `UserDomains` | UserId (NVARCHAR(450)), TopicId (INT), Score (DECIMAL) |
| `Goals` | GoalId (BIGINT), UserId (NVARCHAR(450)), Title (NVARCHAR), Priority (INT), CreatedAt |

### 3. **Default Values Added to Procedures**
- `usp_UpdateMastery`: Now explicitly sets `LastUpdated = GETUTCDATE()` when creating record
- `usp_EnsureUserTopicMastery`: Now explicitly sets `LastUpdated = GETUTCDATE()`
- `usp_GetOrCreateResource`: Now sets `CreatedAt = GETUTCDATE()` for new resources

### 4. **Procedures Overview**

#### `usp_ComputeEvidenceScore`
- Aggregates Evidence rows (quiz, study_time, retention_test) into single score
- Weighted: 0.50 (quiz) + 0.25 (study) + 0.25 (retention)
- Returns composite score [0.00-1.00]

#### `usp_UpdateMastery`
- Updates user skill mastery using EMA (Exponential Moving Average)
- Formula: NewMastery = OldMastery * 0.80 + EvidenceScore * 0.20
- Increments EvidenceCount on update

#### `usp_RecalcDomainScore`
- Calculates Domain proficiency as weighted average of child topics
- Uses TopicRelationships (type='contains') and weights
- Upserts into UserDomains

#### `usp_ComputeConfidenceScore`
- Composite formula: 0.30*SessionScore + 0.30*AssessmentScore + 0.20*RecencyScore + 0.20*ConsistencyScore
- SessionScore: min(distinct_sessions / 20, 1.0)
- AssessmentScore: min(assessments / 10, 1.0)
- RecencyScore: exp(-days_since_study / 60)
- ConsistencyScore: 1.0 - normalized_std_dev

#### `usp_ProcessSession` (Orchestrator)
- Runs full pipeline in order:
  1. Compute evidence score
  2. Update mastery
  3. Compute confidence
  4. Recalc domain score

#### `usp_GetRecommendationPriorities`
- Returns top 20 learning recommendations
- Priority = (RequiredMastery - CurrentMastery) × Confidence × Interest × CareerRelevance
- Joins with Goals to calculate career relevance

#### `usp_EnsureUserTopicMastery`
- Creates UserTopicMastery record with defaults if missing
- Used as guard before mastery updates

#### `usp_GetOrCreateResource`
- Gets existing Resource by URL or creates new one
- Returns ResourceId for linking to StudySessions

#### `usp_UpsertTopic`
- Gets existing Topic by Name or creates new one
- Default Difficulty = 1, EstimatedHours = 4.00
- Returns TopicId and Created flag

### 5. **View**
#### `UserSkillsView`
- User-friendly view showing user skills/topics
- Columns: UserId, TopicId, SkillName, Proficiency (Mastery), Confidence, EvidenceCount, LastUpdated

## Files Created
- `UpdatedStoredProceduresAndView.sql` - Complete SQL script with all procedures and view

## Next Steps
1. Execute the SQL script in your StuckIn database
2. Create C# service wrappers in BL layer to call these procedures
3. The migration will handle table creation; these procedures are optional enhancements for other projects accessing the database
