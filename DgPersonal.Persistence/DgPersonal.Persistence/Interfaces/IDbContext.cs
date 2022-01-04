using System;
using System.Linq;
using System.Threading.Tasks;
using DgPersonal.Persistence.Classes;
using Microsoft.EntityFrameworkCore;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IDbContext : IDbContextNet5
    {
        DbSet<Audit> Audit { get; set; }
        IQueryable<T> GetDbSet<T>() where T : class;
        Task Transaction(Func<Task> transactionBody);
    }
}