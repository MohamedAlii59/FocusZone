using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;


namespace DAL.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }

        List<Expression<Func<T, object>>> Includes { get; }

        List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> IncludeExpressions { get; }

        Expression<Func<T, object>> OrderBy { get; }

        Expression<Func<T, object>> OrderByDescending { get; }

        int Skip { get; }

        int Take { get; }

        bool IsPagingEnabled { get; }
    }
}
