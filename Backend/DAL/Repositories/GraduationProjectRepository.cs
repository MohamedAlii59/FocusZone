using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Database;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// Repository for all graduation project operations (merged database)
    /// Provides abstraction layer for Topics, Resources, StudySessions, and Evidence
    /// </summary>
    public class GraduationProjectRepository
    {
        private readonly AppDbContext _context;

        public GraduationProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============= TOPIC OPERATIONS =============

        public async Task<Topic> GetOrCreateTopicAsync(string topicName, string topicType = "Concept")
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == topicName);

            if (topic != null)
                return topic;

            topic = new Topic
            {
                Name = topicName,
                Type = topicType,
                Difficulty = 2,
                EstimatedHours = 4.00m
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();
            return topic;
        }

        public async Task<Topic> GetTopicByNameAsync(string topicName)
        {
            return await _context.Topics.FirstOrDefaultAsync(t => t.Name == topicName);
        }

        public async Task<Topic> GetTopicByIdAsync(int topicId)
        {
            return await _context.Topics.FindAsync(topicId);
        }

        public async Task<IEnumerable<Topic>> GetAllTopicsAsync()
        {
            return await _context.Topics.ToListAsync();
        }

        public async Task<Topic> CreateTopicAsync(string name, string description, string type, int difficulty, decimal estimatedHours)
        {
            var topic = new Topic
            {
                Name = name,
                Description = description,
                Type = type,
                Difficulty = difficulty,
                EstimatedHours = estimatedHours
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();
            return topic;
        }

        // ============= RESOURCE OPERATIONS =============

        public async Task<Resource> GetOrCreateResourceAsync(string url, string title, float durationMinutes, 
            string sourceType = "youtube", int difficulty = 2)
        {
            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Url == url);

            if (resource != null)
                return resource;

            resource = new Resource
            {
                Title = title,
                Type = sourceType.ToUpper() == "YOUTUBE" ? "Youtube" : sourceType,
                Url = url,
                Difficulty = difficulty,
                Depth = 2,
                EstimatedMinutes = (int)durationMinutes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            return resource;
        }

        public async Task<Resource> GetResourceByIdAsync(long resourceId)
        {
            return await _context.Resources
                .Include(r => r.TopicCoverages)
                .FirstOrDefaultAsync(r => r.ResourceId == resourceId);
        }

        public async Task<Resource> GetResourceByUrlAsync(string url)
        {
            return await _context.Resources.FirstOrDefaultAsync(r => r.Url == url);
        }

        public async Task<IEnumerable<Resource>> GetAllResourcesAsync()
        {
            return await _context.Resources.ToListAsync();
        }

        // ============= STUDY SESSION OPERATIONS =============

        public async Task<StudySession> CreateStudySessionAsync(string userId, long? resourceId, 
            string summary, float durationMinutes)
        {
            var now = DateTime.UtcNow;
            var session = new StudySession
            {
                UserId = userId,
                ResourceId = resourceId,
                StartedAt = now,
                EndedAt = now.AddMinutes(durationMinutes),
                SessionSummary = summary
            };

            _context.StudySessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<StudySession> GetStudySessionByIdAsync(long sessionId)
        {
            return await _context.StudySessions
                .Include(ss => ss.Evidence)
                .Include(ss => ss.Questions)
                .FirstOrDefaultAsync(ss => ss.SessionId == sessionId);
        }

        public async Task<StudySession> GetSessionForUserResourceAsync(string userId, long resourceId)
        {
            return await _context.StudySessions
                .FirstOrDefaultAsync(ss => ss.UserId == userId && ss.ResourceId == resourceId);
        }

        public async Task<IEnumerable<StudySession>> GetUserSessionsAsync(string userId)
        {
            return await _context.StudySessions
                .Where(ss => ss.UserId == userId)
                .Include(ss => ss.Resource)
                .OrderByDescending(ss => ss.StartedAt)
                .ToListAsync();
        }

        // ============= EVIDENCE OPERATIONS =============

        public async Task<Evidence> CreateEvidenceAsync(long sessionId, int topicId, string type, decimal score)
        {
            if (score < 0 || score > 1)
                throw new ArgumentException("Score must be between 0.00 and 1.00");

            var evidence = new Evidence
            {
                SessionId = sessionId,
                TopicId = topicId,
                Type = type,
                Score = Math.Round(score, 2),
                CreatedAt = DateTime.UtcNow
            };

            _context.Evidence.Add(evidence);
            await _context.SaveChangesAsync();
            return evidence;
        }

        public async Task<IEnumerable<Evidence>> GetSessionEvidenceAsync(long sessionId)
        {
            return await _context.Evidence
                .Where(e => e.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evidence>> GetTopicEvidenceForUserAsync(string userId, int topicId)
        {
            return await _context.Evidence
                .Where(e => e.TopicId == topicId && e.StudySession.UserId == userId)
                .ToListAsync();
        }

        // ============= TOPIC RELATIONSHIP OPERATIONS =============

        public async Task<TopicRelationship> CreateTopicRelationshipAsync(int sourceTopicId, int targetTopicId, 
            string relationshipType)
        {
            // Check for duplicate
            var existing = await _context.TopicRelationships
                .FirstOrDefaultAsync(tr => tr.SourceTopicId == sourceTopicId && 
                                          tr.TargetTopicId == targetTopicId && 
                                          tr.RelationshipType == relationshipType);

            if (existing != null)
                return existing;

            var relationship = new TopicRelationship
            {
                SourceTopicId = sourceTopicId,
                TargetTopicId = targetTopicId,
                RelationshipType = relationshipType,
                Weight = 1.00m
            };

            _context.TopicRelationships.Add(relationship);
            await _context.SaveChangesAsync();
            return relationship;
        }

        public async Task<IEnumerable<TopicRelationship>> GetTopicRelationshipsAsync(int topicId)
        {
            return await _context.TopicRelationships
                .Where(tr => tr.SourceTopicId == topicId)
                .Include(tr => tr.TargetTopic)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetPrerequisiteTopicsAsync(int targetTopicId)
        {
            return await _context.TopicRelationships
                .Where(tr => tr.TargetTopicId == targetTopicId && 
                            tr.RelationshipType == "prerequisite_for")
                .Select(tr => tr.SourceTopicId)
                .ToListAsync();
        }

        // ============= USER TOPIC MASTERY OPERATIONS =============

        public async Task<UserTopicMastery> GetOrCreateUserTopicMasteryAsync(string userId, int topicId)
        {
            var mastery = await _context.UserTopicMasteries
                .FirstOrDefaultAsync(utm => utm.UserId == userId && utm.TopicId == topicId);

            if (mastery != null)
                return mastery;

            mastery = new UserTopicMastery
            {
                UserId = userId,
                TopicId = topicId,
                Mastery = 0.00m,
                Confidence = 0.00m,
                Interest = 0.50m,
                EvidenceCount = 0,
                LastUpdated = DateTime.UtcNow
            };

            _context.UserTopicMasteries.Add(mastery);
            await _context.SaveChangesAsync();
            return mastery;
        }

        public async Task<UserTopicMastery> GetUserTopicMasteryAsync(string userId, int topicId)
        {
            return await _context.UserTopicMasteries
                .FirstOrDefaultAsync(utm => utm.UserId == userId && utm.TopicId == topicId);
        }

        public async Task<IEnumerable<UserTopicMastery>> GetUserMasteriesAsync(string userId)
        {
            return await _context.UserTopicMasteries
                .Where(utm => utm.UserId == userId)
                .Include(utm => utm.Topic)
                .OrderByDescending(utm => utm.Mastery)
                .ToListAsync();
        }

        public async Task UpdateUserTopicMasteryAsync(string userId, int topicId, decimal mastery, 
            decimal confidence, int evidenceCount)
        {
            var utm = await GetOrCreateUserTopicMasteryAsync(userId, topicId);
            utm.Mastery = Math.Min(1.00m, Math.Max(0.00m, mastery));
            utm.Confidence = Math.Min(1.00m, Math.Max(0.00m, confidence));
            utm.EvidenceCount = evidenceCount;
            utm.LastUpdated = DateTime.UtcNow;

            _context.UserTopicMasteries.Update(utm);
            await _context.SaveChangesAsync();
        }

        // ============= USER DOMAIN OPERATIONS =============

        public async Task<UserDomain> GetOrCreateUserDomainAsync(string userId, int domainTopicId)
        {
            var domain = await _context.UserDomains
                .FirstOrDefaultAsync(ud => ud.UserId == userId && ud.TopicId == domainTopicId);

            if (domain != null)
                return domain;

            domain = new UserDomain
            {
                UserId = userId,
                TopicId = domainTopicId,
                Score = 0.00m
            };

            _context.UserDomains.Add(domain);
            await _context.SaveChangesAsync();
            return domain;
        }

        public async Task<UserDomain> GetUserDomainAsync(string userId, int domainTopicId)
        {
            return await _context.UserDomains
                .FirstOrDefaultAsync(ud => ud.UserId == userId && ud.TopicId == domainTopicId);
        }

        public async Task UpdateUserDomainScoreAsync(string userId, int domainTopicId, decimal score)
        {
            var domain = await GetOrCreateUserDomainAsync(userId, domainTopicId);
            domain.Score = Math.Min(1.00m, Math.Max(0.00m, score));

            _context.UserDomains.Update(domain);
            await _context.SaveChangesAsync();
        }

        // ============= GOAL OPERATIONS (merged interests + goals) =============

        public async Task<Goal> CreateGoalAsync(string userId, string title, string description = null, 
            int priority = 1, string category = "Goal")
        {
            var goal = new Goal
            {
                UserId = userId,
                Title = title,
                Description = description,
                Priority = priority,
                Category = category,
                CreatedAt = DateTime.UtcNow
            };

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();
            return goal;
        }

        public async Task<IEnumerable<Goal>> GetUserGoalsAsync(string userId, string category = null)
        {
            var query = _context.Goals.Where(g => g.UserId == userId);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(g => g.Category == category);

            return await query.OrderByDescending(g => g.Priority).ToListAsync();
        }

        public async Task<Goal> GetGoalByIdAsync(long goalId)
        {
            return await _context.Goals.FindAsync(goalId);
        }

        public async Task DeleteGoalAsync(long goalId)
        {
            var goal = await _context.Goals.FindAsync(goalId);
            if (goal != null)
            {
                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
            }
        }

        // ============= RESOURCE TOPIC COVERAGE =============

        public async Task<ResourceTopicCoverage> CreateResourceTopicCoverageAsync(long resourceId, int topicId, 
            decimal coverageWeight = 1.00m, decimal difficultyContribution = 1.00m)
        {
            var coverage = new ResourceTopicCoverage
            {
                ResourceId = resourceId,
                TopicId = topicId,
                CoverageWeight = coverageWeight,
                DifficultyContribution = difficultyContribution
            };

            _context.ResourceTopicCoverages.Add(coverage);
            await _context.SaveChangesAsync();
            return coverage;
        }

        public async Task<IEnumerable<Topic>> GetResourceTopicsAsync(long resourceId)
        {
            return await _context.ResourceTopicCoverages
                .Where(rtc => rtc.ResourceId == resourceId)
                .Include(rtc => rtc.Topic)
                .Select(rtc => rtc.Topic)
                .ToListAsync();
        }

        // ============= QUESTION OPERATIONS =============

        public async Task<Question> CreateQuestionAsync(string userId, long sessionId, string questionText)
        {
            var question = new Question
            {
                UserId = userId,
                SessionId = sessionId,
                QuestionText = questionText,
                CreatedAt = DateTime.UtcNow
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<IEnumerable<Question>> GetSessionQuestionsAsync(long sessionId)
        {
            return await _context.Questions
                .Where(q => q.SessionId == sessionId)
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();
        }
    }
}
