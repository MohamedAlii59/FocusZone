using AutoMapper;
using BL.DTOs.UserTopicMastery;
using BL.Pagination;
using BL.Services.Abstraction;
using BL.Specifications;
using BL.Specifications.UserTopicsMastery;
using DAL.Entities;
using DAL.Repositories.Abstraction;

namespace BL.Services.Implementation
{
    public class UserTopicMasteryService : IUserTopicMasteryService
    {
        private readonly IGenericRepository<UserTopicMastery> _repo;
        private readonly IMapper _mapper;

        public UserTopicMasteryService(IGenericRepository<UserTopicMastery> repo,IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaginationResponse<UserTopicMasteryDto>> GetByUserAsync(string userId,PaginationParams pagination)
        {
            var spec = new UserTopicMasteryByUserSpec(userId, pagination);

            var data = await _repo.GetAllAsync(spec);

            var count = await _repo.CountAsync(new BaseSpecification<UserTopicMastery>(x => x.UserId == userId));

            return new PaginationResponse<UserTopicMasteryDto>
            {
                Count = count,
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize,
                Data = _mapper.Map<IReadOnlyList<UserTopicMasteryDto>>(data)
            };
        }

        public async Task CreateAsync(UserTopicMasteryDto dto)
        {
            var entity = _mapper.Map<UserTopicMastery>(dto);

            await _repo.AddAsync(entity);

            await _repo.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserTopicMasteryDto dto)
        {
            var spec = new UserTopicMasteryByIdSpecification(dto.UserId, dto.TopicId);

            var entity = await _repo.FirstOrDefaultAsync(spec);

            if (entity == null) throw new Exception("Not Found");

            _mapper.Map(dto, entity);

            _repo.Update(entity);

            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(string userId, int topicId)
        {
            var spec = new UserTopicMasteryByIdSpecification(userId, topicId);

            var entity = await _repo.FirstOrDefaultAsync(spec);

            if (entity == null) throw new Exception("Not Found");

            _repo.Delete(entity);

            await _repo.SaveChangesAsync();
        }
    }
}
