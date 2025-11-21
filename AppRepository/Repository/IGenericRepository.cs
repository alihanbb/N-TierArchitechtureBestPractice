using System.Linq.Expressions;
namespace AppRepository.Repository
{
    public interface IGenericRepository<T,TId> where T : class where TId : struct
    {
        Task<bool> AnyAsync(TId id);
        // generic repository'i çok fazla metodları çok şişirmemek için sadece CRUD işlemleri yapacak şekilde tasarladık.   
        IQueryable<T> GetAll();
        ValueTask<T?> GetByIdAsync(TId id);
        IQueryable<T> where(Expression<Func<T, bool>> predicate);
        ValueTask AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
