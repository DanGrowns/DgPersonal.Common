using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using DgPersonal.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using TinyCqrs.Classes;
using TinyFluentValidator.Classes;

namespace DgPersonal.Common.Classes
{
    public class EntityFrameworkModelDeleter : IEntityFrameworkModelDeleter
    {
        private object PrimaryKey { get; set; }
        private Type EntityType { get; set; }
        private IDbContext DbContext { get; }
        
        public EntityFrameworkModelDeleter(IDbContext dbContext)
            => DbContext = dbContext;

        private async Task<object> GetEntityByKey()
            => await DbContext.FindAsync(EntityType, PrimaryKey);

        private async Task<object> GetEntityByPredicate<TEntity>(Expression<Func<TEntity, bool>> predicate) 
            where TEntity : class, IEntity
        {
            var entity = await DbContext.GetDbSet<TEntity>().FirstOrDefaultAsync(predicate);
            PrimaryKey = entity.GetPrimaryKey();

            return entity;
        }

        private async Task<CmdResult> Delete(int changedBy, Func<Task<object>> getEntity)
        {
            var cmdResult = new CmdResult($"Delete {EntityType.Name.SplitPascalCaseToString()}");

            try
            {
                var existingEntity = await getEntity();
                if (existingEntity == null)
                {
                    cmdResult.AddIssue("The entity you are trying to delete does not exist.");
                }
                else
                {
                    DbContext.Remove(existingEntity);
                    
                    var audit = 
                        new Audit("Delete", 
                            existingEntity.GetType().Name, 
                            PrimaryKey.ToString(), 
                            JsonSerializer.Serialize(existingEntity),
                            changedBy);

                    await DbContext.Audit.AddAsync(audit);
                    await DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                cmdResult.AddIssue(ex.InnerException != null
                    ? $"{ex.Message}. [Inner exception] {ex.InnerException.Message}"
                    : ex.Message);
            }

            return cmdResult;
        }
        
        public async Task<CmdResult> Delete<TEntity>(TEntity entity, int changedBy)
            where TEntity : class, IEntity
        {
            EntityType = typeof(TEntity);
            return await Delete<TEntity>(entity.GetPrimaryKey(), changedBy);
        }
        
        public async Task<CmdResult> Delete<TEntity>(object keyValue, int changedBy)
            where TEntity : class, IEntity
            => await Delete(typeof(TEntity), keyValue, changedBy);
        
        public async Task<CmdResult> Delete(Type entityType, object primaryKey, int changedBy)
        {
            EntityType = entityType;
            PrimaryKey = primaryKey;
            
            return await Delete(changedBy, async () => await GetEntityByKey());
        }
        
        public async Task<CmdResult> Delete<TEntity>(Expression<Func<TEntity, bool>> predicate, int changedBy)
            where TEntity : class, IEntity
        {
            EntityType = typeof(TEntity);
            return await Delete(changedBy, async () => await GetEntityByPredicate(predicate));
        }
    }
}