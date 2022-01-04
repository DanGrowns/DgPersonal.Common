using System;
using System.Linq;
using System.Threading.Tasks;
using DgPersonal.Common.Classes;
using Microsoft.EntityFrameworkCore;

namespace DgPersonal.Common.Interfaces
{
    public interface IDbContext : IDbContextNet5
    {
        DbSet<Audit> Audit { get; set; }
        IQueryable<T> GetDbSet<T>() where T : class;
        Task Transaction(Func<Task> transactionBody);
    }
}