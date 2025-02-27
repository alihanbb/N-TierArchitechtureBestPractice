using System.Linq.Expressions;
namespace AppRepository.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        // generic repository'i çok fazla metodları çok şişirmemek için sadece CRUD işlemleri yapacak şekilde tasarladık.   
        IQueryable<T> GetAll();
        ValueTask<T> GetByIdAsync(int id);
        IQueryable<T> where(Expression<Func<T, bool>> predicate);
        ValueTask AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
