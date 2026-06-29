using AutoMapper;
using BL.DTOs.ExamSession;
using BL.Pagination;
using BL.Services.Abstraction;
using BL.Specifications.Exam;
using DAL.Entities;
using DAL.Repositories.Abstraction;

namespace BL.Services.Implementation
{
    public class ExamSessionService : IExamSessionService
    {
        private readonly IGenericRepository<ExamSession> _repository;
        private readonly IMapper _mapper;

        public ExamSessionService(IGenericRepository<ExamSession> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginationResponse<ExamSessionDto>> GetByUserAsync( string userId, PaginationParams pagination,string? search = null,string? type=null)
        {
            var spec = new ExamSessionByUserSpecification(userId, pagination, search,type);

            var items = await _repository.GetAllAsync(spec);

            var count = await _repository.CountAsync(
                new ExamSessionCountSpecification(userId, search,type));

            return new PaginationResponse<ExamSessionDto>
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize,
                Count = count,
                Data = _mapper.Map<IReadOnlyList<ExamSessionDto>>(items)
            };
        }

        public async Task<ExamSessionDto?> GetAsync(int id)
        {
            var spec = new ExamSessionByIdSpecification(id);

            var entity = await _repository.FirstOrDefaultAsync(spec);

            return entity == null? null : _mapper.Map<ExamSessionDto>(entity);
        }

        public async Task<ExamSessionDto?> GetBySessionAndUserAsync( long sessionId,string userId)
        {
            var spec = new ExamSessionBySessionAndUserSpecification(
                sessionId,
                userId);

            var entity = await _repository.FirstOrDefaultAsync(spec);

            return entity == null? null : _mapper.Map<ExamSessionDto>(entity);
        }

        public async Task<ExamSessionDto> CreateAsync(AddExamSessionDto dto)
        {
            var entity = _mapper.Map<ExamSession>(dto);

            entity.ExamId = 0;

            if (entity.SessionAnswers != null)
            {
                foreach (var answer in entity.SessionAnswers)
                {
                    answer.AnswerId = 0;

                    if (answer.AnswerChoices != null)
                    {
                        foreach (var choice in answer.AnswerChoices)
                        {
                            choice.ChoiceId = 0;
                        }
                    }
                }
            }

            await _repository.AddAsync(entity);

            await _repository.SaveChangesAsync();

            return _mapper.Map<ExamSessionDto>(entity);
        }

        public async Task UpdateAsync(int id, ExamSessionDto dto)
        {
            if (id != dto.ExamId)
                throw new Exception("Invalid Exam Id.");

            var spec = new ExamSessionByIdSpecification(id);

            var entity = await _repository.FirstOrDefaultAsync(spec);

            if (entity == null)
                throw new KeyNotFoundException("Exam Session not found.");

            entity.Score = dto.Score;
            entity.TotalQuestions = dto.TotalQuestions;
            entity.Passed = dto.Passed;
            entity.SessionId = dto.SessionId;

            entity.SessionAnswers.Clear();

            if (dto.SessionAnswers != null)
            {
                foreach (var sa in dto.SessionAnswers)
                {
                    entity.SessionAnswers.Add(new SessionAnswer
                    {
                        Topic = sa.Topic,
                        Question = sa.Question,
                        UserAnswer = sa.UserAnswer,
                        CorrectAnswer = sa.CorrectAnswer,
                        IsCorrect = sa.IsCorrect,
                        Explanation = sa.Explanation,

                        AnswerChoices = sa.AnswerChoices?
                            .Select(x => new AnswerChoice
                            {
                                ChoiceKey = x.ChoiceKey,
                                ChoiceValue = x.ChoiceValue
                            }).ToList()
                    });
                }
            }

            _repository.Update(entity);

            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var spec = new ExamSessionByIdSpecification(id);

            var entity = await _repository.FirstOrDefaultAsync(spec);

            if (entity == null)
                throw new KeyNotFoundException("Exam Session not found.");

            _repository.Delete(entity);

            await _repository.SaveChangesAsync();
        }
    }
}
