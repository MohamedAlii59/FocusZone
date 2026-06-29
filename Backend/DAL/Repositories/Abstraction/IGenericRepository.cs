using DAL.Specifications;


namespace DAL.Repositories.Abstraction
{
    public interface IGenericRepository<T> where T : class
    {
      


        Task<IReadOnlyList<T>> GetAllAsync(ISpecification<T> spec);

        Task<int> CountAsync(ISpecification<T> spec);

        Task<T?> FirstOrDefaultAsync(ISpecification<T> spec);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task SaveChangesAsync();
    }
}
