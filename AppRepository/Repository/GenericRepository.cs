using AppRepository.Context;

namespace AppRepository.Repository
{
    public class GenericRepository<T,TId>(AppDbContextcs contextcs) 
        : IGenericRepository<T,TId> where T : BaseEntity<TId> where TId : struct
    {
        protected AppDbContextcs Contextcs = contextcs;
        private readonly DbSet<T> _dbSet = contextcs.Set<T>();
        public Task<bool> AnyAsync(TId id) => _dbSet.AnyAsync(x => x.Id.Equals(id));
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
        public ValueTask<T?> GetByIdAsync(TId id)
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
