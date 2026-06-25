using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Database
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<Evidence> Evidence { get; set; }
        public DbSet<TopicRelationship> TopicRelationships { get; set; }
        public DbSet<UserTopicMastery> UserTopicMasteries { get; set; }
        public DbSet<UserDomain> UserDomains { get; set; }
        public DbSet<ResourceTopicCoverage> ResourceTopicCoverages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Goal> Goals { get; set; }

        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ExamSession> ExamSessions { get; set; }
        public DbSet<SessionAnswer> SessionAnswers { get; set; }
        public DbSet<AnswerChoice> AnswerChoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>()
     .HasOne(u => u.Country)
     .WithMany(c => c.Users)
     .HasForeignKey(u => u.CountryId)
     .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<User>()
                .HasOne(u => u.Governorate)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<User>()
                .HasOne(u => u.City)
                .WithMany()
                .HasForeignKey(u => u.CityId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserInterests)
                .WithOne(ui => ui.User)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Country Configuration
            modelBuilder.Entity<Country>()
                .HasMany(c => c.Governorates)
                .WithOne(g => g.Country)
                .HasForeignKey(g => g.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Governorate Configuration
            modelBuilder.Entity<Governorate>()
                .HasMany(g => g.Cities)
                .WithOne(c => c.Governorate)
                .HasForeignKey(c => c.GovernorateId)
                .OnDelete(DeleteBehavior.Cascade);

            // User Interest Configuration
            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserInterests)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Subscription Configuration
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscriptionPlan)
                .WithMany(sp => sp.Subscriptions)
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment Configuration
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.SubscriptionPlan)
                .WithMany()
                .HasForeignKey(p => p.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // SubscriptionPlan Configuration
            modelBuilder.Entity<SubscriptionPlan>()
                .Property(sp => sp.Price)
                .HasPrecision(18, 2);

            // Global Query Filter for soft delete
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.IsDeleted);

            modelBuilder.Entity<Goal>().HasQueryFilter(g => !g.User.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.User.IsDeleted);
            modelBuilder.Entity<Question>().HasQueryFilter(q => !q.User.IsDeleted);
            modelBuilder.Entity<StudySession>().HasQueryFilter(ss => !ss.User.IsDeleted);
            modelBuilder.Entity<Subscription>().HasQueryFilter(s => !s.User.IsDeleted);
            modelBuilder.Entity<UserDomain>().HasQueryFilter(ud => !ud.User.IsDeleted);
            modelBuilder.Entity<UserInterest>().HasQueryFilter(ui => !ui.User.IsDeleted);
            modelBuilder.Entity<UserTopicMastery>().HasQueryFilter(utm => !utm.User.IsDeleted);
            modelBuilder.Entity<Evidence>().HasQueryFilter(e => !e.StudySession.User.IsDeleted);
            modelBuilder.Entity<Project>().HasQueryFilter(es => !es.User.IsDeleted);
            modelBuilder.Entity<Experience>().HasQueryFilter(es => !es.User.IsDeleted);
            modelBuilder.Entity<Education>().HasQueryFilter(es => !es.User.IsDeleted);
            modelBuilder.Entity<Certificate>().HasQueryFilter(es => !es.User.IsDeleted);

            // ============= NEW ENTITIES FOR GRADUATION PROJECT =============

            // Topic Configuration
            modelBuilder.Entity<Topic>()
                .HasKey(t => t.TopicId);
            
            modelBuilder.Entity<Topic>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<Topic>()
                .Property(t => t.EstimatedHours)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Topic>()
                .Property(t => t.Difficulty)
                .HasDefaultValue(1);

            // Resource Configuration
            modelBuilder.Entity<Resource>()
                .HasKey(r => r.ResourceId);

            modelBuilder.Entity<Resource>()
                .HasMany(r => r.StudySessions)
                .WithOne(ss => ss.Resource)
                .HasForeignKey(ss => ss.ResourceId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Resource>()
                .HasMany(r => r.TopicCoverages)
                .WithOne(rtc => rtc.Resource)
                .HasForeignKey(rtc => rtc.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Resource>()
                .Property(r => r.Difficulty)
                .HasDefaultValue(1);

            modelBuilder.Entity<Resource>()
                .Property(r => r.Depth)
                .HasDefaultValue(1);

            modelBuilder.Entity<Resource>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("getutcdate()");

            // StudySession Configuration
            modelBuilder.Entity<StudySession>()
                .HasKey(ss => ss.SessionId);

            modelBuilder.Entity<StudySession>()
                .HasOne(ss => ss.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudySession>()
                .HasOne(ss => ss.Resource)
                .WithMany(r => r.StudySessions)
                .HasForeignKey(ss => ss.ResourceId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<StudySession>()
                .HasMany(ss => ss.Evidence)
                .WithOne(e => e.StudySession)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudySession>()
                .HasMany(ss => ss.Questions)
                .WithOne(q => q.StudySession)
                .HasForeignKey(q => q.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Evidence Configuration
            modelBuilder.Entity<Evidence>()
                .HasKey(e => e.EvidenceId);

            modelBuilder.Entity<Evidence>()
                .Property(e => e.Score)
                .HasPrecision(3, 2);

            modelBuilder.Entity<Evidence>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("getutcdate()");

            modelBuilder.Entity<Evidence>()
                .HasOne(e => e.StudySession)
                .WithMany(ss => ss.Evidence)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Evidence>()
                .HasOne(e => e.Topic)
                .WithMany(t => t.Evidence)
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            // TopicRelationship Configuration
            modelBuilder.Entity<TopicRelationship>()
                .HasKey(tr => tr.RelationshipId);

            modelBuilder.Entity<TopicRelationship>()
                .HasOne(tr => tr.SourceTopic)
                .WithMany(t => t.SourceRelationships)
                .HasForeignKey(tr => tr.SourceTopicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TopicRelationship>()
                .HasOne(tr => tr.TargetTopic)
                .WithMany(t => t.TargetRelationships)
                .HasForeignKey(tr => tr.TargetTopicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TopicRelationship>()
                .Property(tr => tr.Weight)
                .HasPrecision(3, 2)
                .HasDefaultValue(1.00m);

            // UserTopicMastery Configuration
            modelBuilder.Entity<UserTopicMastery>()
                .HasKey(utm => new { utm.UserId, utm.TopicId });

            modelBuilder.Entity<UserTopicMastery>()
                .HasOne(utm => utm.User)
                .WithMany(u => u.TopicMasteries)
                .HasForeignKey(utm => utm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTopicMastery>()
                .HasOne(utm => utm.Topic)
                .WithMany(t => t.UserMasteries)
                .HasForeignKey(utm => utm.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserTopicMastery>()
                .Property(utm => utm.Mastery)
                .HasPrecision(3, 2)
                .HasDefaultValue(0.00m);

            modelBuilder.Entity<UserTopicMastery>()
                .Property(utm => utm.Confidence)
                .HasPrecision(3, 2)
                .HasDefaultValue(0.00m);

            modelBuilder.Entity<UserTopicMastery>()
                .Property(utm => utm.Interest)
                .HasPrecision(3, 2)
                .HasDefaultValue(0.50m);

            modelBuilder.Entity<UserTopicMastery>()
                .Property(utm => utm.EvidenceCount)
                .HasDefaultValue(0);

            modelBuilder.Entity<UserTopicMastery>()
                .Property(utm => utm.LastUpdated)
                .HasDefaultValueSql("getutcdate()");

            // UserDomain Configuration
            modelBuilder.Entity<UserDomain>()
                .HasKey(ud => new { ud.UserId, ud.TopicId });

            modelBuilder.Entity<UserDomain>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.UserDomains)
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDomain>()
                .HasOne(ud => ud.Topic)
                .WithMany(t => t.UserDomains)
                .HasForeignKey(ud => ud.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDomain>()
                .Property(ud => ud.Score)
                .HasPrecision(3, 2);

            // ResourceTopicCoverage Configuration
            modelBuilder.Entity<ResourceTopicCoverage>()
                .HasKey(rtc => new { rtc.ResourceId, rtc.TopicId });

            modelBuilder.Entity<ResourceTopicCoverage>()
                .HasOne(rtc => rtc.Resource)
                .WithMany(r => r.TopicCoverages)
                .HasForeignKey(rtc => rtc.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ResourceTopicCoverage>()
                .HasOne(rtc => rtc.Topic)
                .WithMany(t => t.ResourceCoverages)
                .HasForeignKey(rtc => rtc.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ResourceTopicCoverage>()
                .Property(rtc => rtc.CoverageWeight)
                .HasPrecision(3, 2)
                .HasDefaultValue(1.00m);

            modelBuilder.Entity<ResourceTopicCoverage>()
                .Property(rtc => rtc.DifficultyContribution)
                .HasPrecision(3, 2)
                .HasDefaultValue(1.00m);

            // Question Configuration
            modelBuilder.Entity<Question>()
                .HasKey(q => q.QuestionId);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.StudySession)
                .WithMany(ss => ss.Questions)
                .HasForeignKey(q => q.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .Property(q => q.CreatedAt)
                .HasDefaultValueSql("getutcdate()");

            // Goal Configuration (merged from Goals and UserInterests)
            modelBuilder.Entity<Goal>()
                .HasKey(g => g.GoalId);

            modelBuilder.Entity<Goal>()
                .HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Goal>()
                .Property(g => g.Priority)
                .HasDefaultValue(1);

            modelBuilder.Entity<Goal>()
                .Property(g => g.CreatedAt)
                .HasDefaultValueSql("getutcdate()");

            // Certificate Configuration
            modelBuilder.Entity<Certificate>()
                .HasKey(c => c.CertificateId);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany(u => u.Certificates)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Certificate>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("sysutcdatetime()");

            // Education Configuration
            modelBuilder.Entity<Education>()
                .HasKey(e => e.EducationId);

            modelBuilder.Entity<Education>()
                .HasOne(e => e.User)
                .WithMany(u => u.Educations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Education>()
                .Property(e => e.SortOrder)
                .HasDefaultValue(0);

            modelBuilder.Entity<Education>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("sysutcdatetime()");

            // Experience Configuration
            modelBuilder.Entity<Experience>()
                .HasKey(e => e.ExperienceId);

            modelBuilder.Entity<Experience>()
                .HasOne(e => e.User)
                .WithMany(u => u.Experiences)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Experience>()
                .Property(e => e.Current)
                .HasDefaultValue(false);

            modelBuilder.Entity<Experience>()
                .Property(e => e.SortOrder)
                .HasDefaultValue(0);

            modelBuilder.Entity<Experience>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("sysutcdatetime()");

            // Project Configuration
            modelBuilder.Entity<Project>()
                .HasKey(p => p.ProjectId);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExamSession Configuration
            modelBuilder.Entity<ExamSession>()
                .HasKey(es => es.SessionId);

            modelBuilder.Entity<ExamSession>()
                .Property(es => es.Score)
                .HasPrecision(3, 2);

            modelBuilder.Entity<ExamSession>()
                .ToTable(tb => tb.HasCheckConstraint("CHK_Score_Range", "Score >= 0.00 AND Score <= 1.00"));

            // SessionAnswer Configuration
            modelBuilder.Entity<SessionAnswer>()
                .HasKey(sa => sa.AnswerId);

            modelBuilder.Entity<SessionAnswer>()
                .HasOne(sa => sa.ExamSession)
                .WithMany(es => es.SessionAnswers)
                .HasForeignKey(sa => sa.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionAnswer>()
                .HasMany(sa => sa.AnswerChoices)
                .WithOne(ac => ac.SessionAnswer)
                .HasForeignKey(ac => ac.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            // AnswerChoice Configuration
            modelBuilder.Entity<AnswerChoice>()
                .HasKey(ac => ac.ChoiceId);

            modelBuilder.Entity<AnswerChoice>()
                .HasOne(ac => ac.SessionAnswer)
                .WithMany(sa => sa.AnswerChoices)
                .HasForeignKey(ac => ac.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add Constraints for merged data integrity
            modelBuilder.Entity<Evidence>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Evidence_Score", "Score >= 0.00 AND Score <= 1.00"));

            modelBuilder.Entity<Evidence>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Evidence_Type", "Type IN ('quiz', 'study_time', 'assessment', 'retention_test')"));

            modelBuilder.Entity<Topic>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Topics_Type", "Type IN ('Domain', 'Concept', 'Technique', 'Tool', 'Career')"));

            modelBuilder.Entity<Resource>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Resources_Type", "Type IN ('Youtube', 'Course', 'Book', 'Article', 'PDF', 'Documentation')"));

            modelBuilder.Entity<TopicRelationship>()
                .ToTable(tb => tb.HasCheckConstraint("CK_TopicRel_NoSelfRef", "SourceTopicId <> TargetTopicId"));

            modelBuilder.Entity<TopicRelationship>()
                .ToTable(tb => tb.HasCheckConstraint("CK_TopicRel_Type", "RelationshipType IN ('contains', 'prerequisite_for', 'required_for', 'related_to')"));

            modelBuilder.Entity<UserTopicMastery>()
                .ToTable(tb => tb.HasCheckConstraint("CK_UTM_Mastery", "Mastery >= 0.00 AND Mastery <= 1.00"));

            modelBuilder.Entity<UserTopicMastery>()
                .ToTable(tb => tb.HasCheckConstraint("CK_UTM_Confidence", "Confidence >= 0.00 AND Confidence <= 1.00"));

            modelBuilder.Entity<UserTopicMastery>()
                .ToTable(tb => tb.HasCheckConstraint("CK_UTM_Interest", "Interest >= 0.00 AND Interest <= 1.00"));

            modelBuilder.Entity<UserDomain>()
                .ToTable(tb => tb.HasCheckConstraint("CK_UserDomains_Score", "Score >= 0.00 AND Score <= 1.00"));

            modelBuilder.Entity<ResourceTopicCoverage>()
                .ToTable(tb => tb.HasCheckConstraint("CK_RTC_CoverageWeight", "CoverageWeight >= 0.00 AND CoverageWeight <= 1.00"));

            modelBuilder.Entity<ResourceTopicCoverage>()
                .ToTable(tb => tb.HasCheckConstraint("CK_RTC_DifficultyContribution", "DifficultyContribution >= 0.00 AND DifficultyContribution <= 1.00"));
        }
    }
}
