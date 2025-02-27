using System.Linq.Expressions;
using AppRepository.Context;
using Microsoft.EntityFrameworkCore;

namespace AppRepository.Repository
{
    public class GenericRepository<T>(AppDbContextcs contextcs) : IGenericRepository<T> where T : class
    {
        protected AppDbContextcs Contextcs = contextcs;
        private readonly DbSet<T> _dbSet = contextcs.Set<T>();
        public async ValueTask AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable().AsNoTracking();
        }
        public ValueTask<T> GetByIdAsync(int id)
        {
            return _dbSet.FindAsync(id);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public IQueryable<T> where(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).AsNoTracking();
        }
    }
}
