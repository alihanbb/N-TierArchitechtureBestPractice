using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppRepository.Context;

namespace AppRepository.UnitOfWorks
{
    public class UnitOfWork(AppDbContextcs contextcs) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync()
        {
            return contextcs.SaveChangesAsync();
        }
    }
}
