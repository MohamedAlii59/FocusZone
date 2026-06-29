using BL.Services.Abstraction;
using BL.Specifications.Exam;
using BL.Specifications.UserTopicsMastery;
using DAL.Entities;
using DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services.Implementation
{
    public class UserDashboardService : IUserDashboardService
    {
        private readonly IGenericRepository<ExamSession> _examSessionRepository;
        private readonly IGenericRepository<UserTopicMastery> _topicMasteryRepository;

        public UserDashboardService(
            IGenericRepository<ExamSession> examSessionRepository,
            IGenericRepository<UserTopicMastery> topicMasteryRepository)
        {
            _examSessionRepository = examSessionRepository;
            _topicMasteryRepository = topicMasteryRepository;
        }

        public async Task<UserDashboardDto> GetDashboardAsync(string userId)
        {
            var studySessionsCount = await _examSessionRepository.CountAsync(new ExamSessionCountSpecification(userId,null,null));

            var passedExamsCount = await _examSessionRepository.CountAsync( new PassedExamByUserCountSpecification(userId));

            var userTopicMasteryCount = await _topicMasteryRepository.CountAsync(new UserTopicMasteryByUserCountSpecification(userId));

            var passRate = studySessionsCount == 0? 0 : Math.Round((double)passedExamsCount / studySessionsCount * 100, 1);

            return new UserDashboardDto
            {
                StudySessionsCount = studySessionsCount,
                PassedExamsCount = passedExamsCount,
                UserTopicMasteryCount = userTopicMasteryCount,
                PassRate = passRate
            };
        }
    }
}
