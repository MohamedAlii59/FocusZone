using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Utilities
{
    /// <summary>
    /// Utility class for migrating data from the old GraduationProject database
    /// into the merged AspNetDatabase
    /// </summary>
    public class DataMigrationUtility
    {
        private readonly AppDbContext _context;

        public DataMigrationUtility(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Migrate goals and interests from old system.
        /// Merges Goals and UserInterests into single Goal table with categories
        /// </summary>
        public async Task MigrateGoalsAndInterestsAsync(IEnumerable<(string UserId, string Title, string Category)> goalsAndInterests)
        {
            var goals = new List<Goal>();

            foreach (var item in goalsAndInterests)
            {
                // Check if already exists
                var exists = _context.Goals.Any(g => 
                    g.UserId == item.UserId && 
                    g.Title == item.Title && 
                    g.Category == item.Category);

                if (!exists)
                {
                    goals.Add(new Goal
                    {
                        UserId = item.UserId,
                        Title = item.Title,
                        Category = item.Category,
                        Priority = 1,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (goals.Any())
            {
                _context.Goals.AddRange(goals);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Migrate topics from old system
        /// </summary>
        public async Task MigrateTopicsAsync(IEnumerable<(string Name, string Type, string Description, int Difficulty, decimal EstimatedHours)> topics)
        {
            var topicsToAdd = new List<Topic>();

            foreach (var topic in topics)
            {
                var exists = _context.Topics.Any(t => t.Name == topic.Name);
                if (!exists)
                {
                    topicsToAdd.Add(new Topic
                    {
                        Name = topic.Name,
                        Type = topic.Type,
                        Description = topic.Description,
                        Difficulty = topic.Difficulty,
                        EstimatedHours = topic.EstimatedHours
                    });
                }
            }

            if (topicsToAdd.Any())
            {
                _context.Topics.AddRange(topicsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Migrate resources from old system
        /// </summary>
        public async Task MigrateResourcesAsync(IEnumerable<(string Title, string Type, string Url, int Difficulty, int Depth, int EstimatedMinutes)> resources)
        {
            var resourcesToAdd = new List<Resource>();

            foreach (var resource in resources)
            {
                var exists = _context.Resources.Any(r => r.Url == resource.Url);
                if (!exists)
                {
                    resourcesToAdd.Add(new Resource
                    {
                        Title = resource.Title,
                        Type = resource.Type,
                        Url = resource.Url,
                        Difficulty = resource.Difficulty,
                        Depth = resource.Depth,
                        EstimatedMinutes = resource.EstimatedMinutes,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (resourcesToAdd.Any())
            {
                _context.Resources.AddRange(resourcesToAdd);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Migrate study sessions from old system
        /// </summary>
        public async Task MigrateStudySessionsAsync(IEnumerable<(string UserId, long? ResourceId, DateTime StartedAt, DateTime? EndedAt, string Summary)> sessions)
        {
            var sessionsToAdd = new List<StudySession>();

            foreach (var session in sessions)
            {
                sessionsToAdd.Add(new StudySession
                {
                    UserId = session.UserId,
                    ResourceId = session.ResourceId,
                    StartedAt = session.StartedAt,
                    EndedAt = session.EndedAt,
                    SessionSummary = session.Summary
                });
            }

            if (sessionsToAdd.Any())
            {
                _context.StudySessions.AddRange(sessionsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Migrate evidence from old system
        /// </summary>
        public async Task MigrateEvidenceAsync(IEnumerable<(long SessionId, int TopicId, string Type, decimal Score, DateTime CreatedAt)> evidence)
        {
            var evidenceToAdd = new List<Evidence>();

            foreach (var ev in evidence)
            {
                evidenceToAdd.Add(new Evidence
                {
                    SessionId = ev.SessionId,
                    TopicId = ev.TopicId,
                    Type = ev.Type,
                    Score = Math.Round(ev.Score, 2),
                    CreatedAt = ev.CreatedAt
                });
            }

            if (evidenceToAdd.Any())
            {
                _context.Evidence.AddRange(evidenceToAdd);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Migrate topic relationships from old system
        /// </summary>
        public async Task MigrateTopicRelationshipsAsync(IEnumerable<(int SourceTopicId, int TargetTopicId, string RelationshipType, decimal Weight)> relationships)
        {
            var relationshipsToAdd = new List<TopicRelationship>();

            foreach (var rel in relationships)
            {
                var exists = _context.TopicRelationships.Any(tr =>
                    tr.SourceTopicId == rel.SourceTopicId &&
                    tr.TargetTopicId == rel.TargetTopicId &&
                    tr.RelationshipType == rel.RelationshipType);

                if (!exists)
                {
                    relationshipsToAdd.Add(new TopicRelationship
                    {
                        SourceTopicId = rel.SourceTopicId,
                        TargetTopicId = rel.TargetTopicId,
                        RelationshipType = rel.RelationshipType,
                        Weight = rel.Weight
                    });
                }
            }

            if (relationshipsToAdd.Any())
            {
                _context.TopicRelationships.AddRange(relationshipsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Migrate user topic masteries from old system
        /// </summary>
        public async Task MigrateUserTopicMasteriesAsync(IEnumerable<(string UserId, int TopicId, decimal Mastery, decimal Confidence, decimal Interest, int EvidenceCount)> masteries)
        {
            var masteriesToAdd = new List<UserTopicMastery>();

            foreach (var mastery in masteries)
            {
                var exists = _context.UserTopicMasteries.Any(utm =>
                    utm.UserId == mastery.UserId &&
                    utm.TopicId == mastery.TopicId);

                if (!exists)
                {
                    masteriesToAdd.Add(new UserTopicMastery
                    {
                        UserId = mastery.UserId,
                        TopicId = mastery.TopicId,
                        Mastery = mastery.Mastery,
                        Confidence = mastery.Confidence,
                        Interest = mastery.Interest,
                        EvidenceCount = mastery.EvidenceCount,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            if (masteriesToAdd.Any())
            {
                _context.UserTopicMasteries.AddRange(masteriesToAdd);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Verify migration success by checking data integrity
        /// </summary>
        public async Task<MigrationReport> VerifyMigrationAsync()
        {
            var report = new MigrationReport
            {
                Timestamp = DateTime.UtcNow,
                TopicCount = _context.Topics.Count(),
                ResourceCount = _context.Resources.Count(),
                StudySessionCount = _context.StudySessions.Count(),
                EvidenceCount = _context.Evidence.Count(),
                UserTopicMasteryCount = _context.UserTopicMasteries.Count(),
                GoalCount = _context.Goals.Count(),
                TopicRelationshipCount = _context.TopicRelationships.Count(),
            };

            // Check for orphaned records
            var orphanedEvidence = await _context.Evidence
                .Where(e => e.StudySession == null || e.Topic == null)
                .CountAsync();

            var orphanedSessions = await _context.StudySessions
                .Where(ss => ss.User == null)
                .CountAsync();

            report.OrphanedEvidenceCount = orphanedEvidence;
            report.OrphanedSessionCount = orphanedSessions;
            report.IsHealthy = orphanedEvidence == 0 && orphanedSessions == 0;

            return report;
        }
    }

    public class MigrationReport
    {
        public DateTime Timestamp { get; set; }
        public int TopicCount { get; set; }
        public int ResourceCount { get; set; }
        public int StudySessionCount { get; set; }
        public int EvidenceCount { get; set; }
        public int UserTopicMasteryCount { get; set; }
        public int GoalCount { get; set; }
        public int TopicRelationshipCount { get; set; }
        public int OrphanedEvidenceCount { get; set; }
        public int OrphanedSessionCount { get; set; }
        public bool IsHealthy { get; set; }

        public override string ToString()
        {
            return $@"
Migration Report - {Timestamp:yyyy-MM-dd HH:mm:ss}
================================================
Topics:                 {TopicCount}
Resources:              {ResourceCount}
Study Sessions:         {StudySessionCount}
Evidence Records:       {EvidenceCount}
User Topic Masteries:   {UserTopicMasteryCount}
Goals:                  {GoalCount}
Topic Relationships:    {TopicRelationshipCount}

Data Integrity:
Orphaned Evidence:      {OrphanedEvidenceCount}
Orphaned Sessions:      {OrphanedSessionCount}
Health Status:          {(IsHealthy ? "HEALTHY ✓" : "ISSUES FOUND ⚠")}
";
        }
    }
}
