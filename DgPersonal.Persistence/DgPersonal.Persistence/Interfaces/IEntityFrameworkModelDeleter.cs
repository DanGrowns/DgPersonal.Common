using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TinyCqrs.Classes;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IEntityFrameworkModelDeleter
    {
        Task<CmdResult> Delete<TEntity>(object primaryKey, int changedBy)
            where TEntity : class, IEntity;
        
        Task<CmdResult> Delete<TEntity>(TEntity entity, int changedBy) 
            where TEntity : class, IEntity;

        Task<CmdResult> Delete(Type entityType, object primaryKey, int changedBy);

        Task<CmdResult> Delete<TEntity>(Expression<Func<TEntity, bool>> predicate, int changedBy)
            where TEntity : class, IEntity;
    }
}