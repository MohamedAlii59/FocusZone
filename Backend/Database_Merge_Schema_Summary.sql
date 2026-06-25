-- Database Merge Summary - Key Tables Created
-- This script shows what was added to StuckIn database

-- ============================================================
-- NEW TABLES FOR GRADUATION PROJECT (merged into StuckIn)
-- ============================================================

-- Topics table - learning topics with hierarchy support
CREATE TABLE [dbo].[Topics](
    [TopicId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] [nvarchar](150) NOT NULL UNIQUE,
    [Description] [nvarchar](max) NULL,
    [Type] [varchar](50) NOT NULL CHECK (Type IN ('Domain', 'Concept', 'Technique', 'Tool', 'Career')),
    [Difficulty] [int] NOT NULL DEFAULT ((1)),
    [EstimatedHours] [decimal](5, 2) NOT NULL
);

-- Resources table - learning materials
CREATE TABLE [dbo].[Resources](
    [ResourceId] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Title] [nvarchar](255) NOT NULL,
    [Type] [varchar](50) NOT NULL CHECK (Type IN ('Youtube', 'Course', 'Book', 'Article', 'PDF', 'Documentation')),
    [Url] [varchar](2048) NOT NULL,
    [Difficulty] [int] NOT NULL DEFAULT ((1)),
    [Depth] [int] NOT NULL DEFAULT ((1)),
    [EstimatedMinutes] [int] NOT NULL,
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE())
);

-- StudySessions table - user study sessions
CREATE TABLE [dbo].[StudySessions](
    [SessionId] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] [nvarchar](450) NOT NULL,
    [ResourceId] [bigint] NULL,
    [StartedAt] [datetime2](7) NOT NULL,
    [EndedAt] [datetime2](7) NULL,
    [DurationMinutes] AS (DATEDIFF(minute, [StartedAt], [EndedAt])),
    [SessionSummary] [nvarchar](max) NULL,
    FOREIGN KEY([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
    FOREIGN KEY([ResourceId]) REFERENCES [Resources]([ResourceId])
);

-- Evidence table - quiz scores, study time, assessments
CREATE TABLE [dbo].[Evidence](
    [EvidenceId] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SessionId] [bigint] NOT NULL,
    [TopicId] [int] NOT NULL,
    [Type] [varchar](50) NOT NULL CHECK (Type IN ('quiz', 'study_time', 'assessment', 'retention_test')),
    [Score] [decimal](3, 2) NOT NULL CHECK (Score >= 0.00 AND Score <= 1.00),
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
    FOREIGN KEY([SessionId]) REFERENCES [StudySessions]([SessionId]) ON DELETE CASCADE,
    FOREIGN KEY([TopicId]) REFERENCES [Topics]([TopicId])
);

-- TopicRelationships table - relationships between topics
CREATE TABLE [dbo].[TopicRelationships](
    [RelationshipId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SourceTopicId] [int] NOT NULL,
    [TargetTopicId] [int] NOT NULL,
    [RelationshipType] [varchar](50) NOT NULL CHECK (RelationshipType IN ('contains', 'prerequisite_for', 'required_for', 'related_to')),
    [Weight] [decimal](3, 2) NOT NULL DEFAULT ((1.00)) CHECK (Weight >= 0.00 AND Weight <= 1.00),
    CONSTRAINT [CK_TopicRel_NoSelfRef] CHECK (SourceTopicId <> TargetTopicId),
    FOREIGN KEY([SourceTopicId]) REFERENCES [Topics]([TopicId]),
    FOREIGN KEY([TargetTopicId]) REFERENCES [Topics]([TopicId])
);

-- UserTopicMastery table - track user mastery/proficiency
CREATE TABLE [dbo].[UserTopicMastery](
    [UserId] [nvarchar](450) NOT NULL,
    [TopicId] [int] NOT NULL,
    [Mastery] [decimal](3, 2) NOT NULL DEFAULT ((0.00)) CHECK (Mastery >= 0.00 AND Mastery <= 1.00),
    [Confidence] [decimal](3, 2) NOT NULL DEFAULT ((0.00)) CHECK (Confidence >= 0.00 AND Confidence <= 1.00),
    [Interest] [decimal](3, 2) NOT NULL DEFAULT ((0.50)) CHECK (Interest >= 0.00 AND Interest <= 1.00),
    [EvidenceCount] [int] NOT NULL DEFAULT ((0)),
    [LastUpdated] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
    PRIMARY KEY ([UserId], [TopicId]),
    FOREIGN KEY([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
    FOREIGN KEY([TopicId]) REFERENCES [Topics]([TopicId])
);

-- UserDomains table - domain-level scores
CREATE TABLE [dbo].[UserDomains](
    [UserId] [nvarchar](450) NOT NULL,
    [TopicId] [int] NOT NULL,
    [Score] [decimal](3, 2) NOT NULL DEFAULT ((0.00)) CHECK (Score >= 0.00 AND Score <= 1.00),
    PRIMARY KEY ([UserId], [TopicId]),
    FOREIGN KEY([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
    FOREIGN KEY([TopicId]) REFERENCES [Topics]([TopicId])
);

-- ResourceTopicCoverage table - map resources to topics
CREATE TABLE [dbo].[ResourceTopicCoverage](
    [ResourceId] [bigint] NOT NULL,
    [TopicId] [int] NOT NULL,
    [CoverageWeight] [decimal](3, 2) NOT NULL DEFAULT ((1.00)) CHECK (CoverageWeight >= 0.00 AND CoverageWeight <= 1.00),
    [DifficultyContribution] [decimal](3, 2) NOT NULL DEFAULT ((1.00)) CHECK (DifficultyContribution >= 0.00 AND DifficultyContribution <= 1.00),
    PRIMARY KEY ([ResourceId], [TopicId]),
    FOREIGN KEY([ResourceId]) REFERENCES [Resources]([ResourceId]) ON DELETE CASCADE,
    FOREIGN KEY([TopicId]) REFERENCES [Topics]([TopicId])
);

-- Questions table - questions asked during sessions
CREATE TABLE [dbo].[Questions](
    [QuestionId] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] [nvarchar](450) NOT NULL,
    [SessionId] [bigint] NOT NULL,
    [QuestionText] [nvarchar](max) NOT NULL,
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
    FOREIGN KEY([UserId]) REFERENCES [AspNetUsers]([Id]),
    FOREIGN KEY([SessionId]) REFERENCES [StudySessions]([SessionId]) ON DELETE CASCADE
);

-- Goals table - MERGED goals and interests
CREATE TABLE [dbo].[Goals](
    [GoalId] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] [nvarchar](450) NOT NULL,
    [Title] [nvarchar](200) NOT NULL,
    [Description] [nvarchar](max) NULL,
    [Priority] [int] NOT NULL DEFAULT ((1)),
    [Category] [nvarchar](50) NULL,  -- Used to distinguish types (Goal, Interest, etc.)
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
    FOREIGN KEY([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
);

-- ============================================================
-- KEY FOREIGN KEY RELATIONSHIPS
-- ============================================================

-- All new tables reference AspNetUsers by UserId (string)
-- This ensures all graduation project data is tied to authenticated users

-- StudySessions -> AspNetUsers (cascade delete)
-- StudySessions -> Resources (restrict delete)
-- Evidence -> StudySessions (cascade delete)
-- Evidence -> Topics (restrict delete)
-- UserTopicMastery -> AspNetUsers (cascade delete)
-- UserTopicMastery -> Topics (restrict delete)
-- UserDomains -> AspNetUsers (cascade delete)
-- UserDomains -> Topics (restrict delete)
-- ResourceTopicCoverage -> Resources (cascade delete)
-- ResourceTopicCoverage -> Topics (restrict delete)
-- Questions -> AspNetUsers (restrict delete)
-- Questions -> StudySessions (cascade delete)
-- Goals -> AspNetUsers (cascade delete)

-- ============================================================
-- HELPFUL VIEWS
-- ============================================================

-- UserSkillsView - Get user's skills with proficiency
CREATE VIEW [dbo].[UserSkillsView] AS
SELECT
    UTM.UserId,
    T.TopicId,
    T.Name AS SkillName,
    UTM.Mastery AS Proficiency,
    UTM.Confidence,
    UTM.EvidenceCount,
    UTM.LastUpdated
FROM UserTopicMastery UTM
JOIN Topics T ON UTM.TopicId = T.TopicId;

-- ============================================================
-- MIGRATION NOTES
-- ============================================================

/*
Key Points for Successful Merge:

1. USER ID CHANGE
   - Old system: int (from old GraduationProject.Users.UserId)
   - New system: nvarchar(450) (from AspNetUsers.Id)
   - All foreign keys reference AspNetUsers.Id

2. GOALS/INTERESTS CONSOLIDATION
   - Old: Separate Goals and UserInterests tables
   - New: Single Goals table with Category column
   - Use Category = 'Goal' or Category = 'Interest' to distinguish

3. REFERENTIAL INTEGRITY
   - All study data is tied to AspNetUsers
   - Cascade deletes on user deletion
   - Restrict deletes on Topics to preserve hierarchy

4. NO QDRANT CHANGES
   - Qdrant collections remain independent
   - No schema changes needed in vector database
   - Python tools updated to use merged connection string

5. DATA TYPES
   - Mastery/Confidence/Interest: decimal(3,2) bounded [0.00-1.00]
   - Scores: decimal(3,2) bounded [0.00-1.00]
   - UserIds: nvarchar(450) from Identity
   - TopicIds: int (auto-increment)
   - ResourceIds: bigint (auto-increment)

6. BACKWARD COMPATIBILITY
   - All original AspNetDatabase tables still exist
   - Authentication unchanged
   - Soft delete flag preserved
   - Subscriptions/Payments unchanged
*/
