using AppRepository.Context;

namespace AppRepository.UnitOfWorks
{
    public class UnitOfWork(AppDbContextcs contextcs) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync()
        {
            return await contextcs.SaveChangesAsync();
        }
    }
}
