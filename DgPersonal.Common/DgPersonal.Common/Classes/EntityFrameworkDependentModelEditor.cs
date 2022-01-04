using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DgPersonal.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using TinyCqrs.Classes;
using TinyFluentValidator.Classes;
using TinyFluentValidator.Interfaces;

namespace DgPersonal.Common.Classes
{
    public class EntityFrameworkDependentModelEditor<TEntity, TCmd>  : IEntityFrameworkDependentModelEditor<TEntity, TCmd>
        where TEntity : class, IStateChange<TCmd>, IValidationEntity<TEntity>, IEntity
        where TCmd :  IFindEntity<TEntity>, IFactoryConstruct<TEntity>
    {
        private IDbContext DbContext { get; }
        private CmdResult CmdResult { get; }
        private IValidator<TEntity> Validator { get; }
        private int ChangedBy { get; set; }

        public EntityFrameworkDependentModelEditor(IDbContext dbContext, IValidator<TEntity> validator)
        {
            DbContext = dbContext;
            Validator = validator;
            CmdResult = new CmdResult($"Edit {typeof(TEntity).Name.SplitPascalCaseToString()}");
        }
        
        private bool StateIsValid(TEntity dbModel)
        {
            var internalState = dbModel.StateIsValid(Validator);

            if (internalState.Count < 1) 
                return true;
            
            var cmdErrors = 
                internalState.Select(x => new CmdIssue(CmdResult.SourceName, x));
                
            CmdResult.Issues.AddRange(cmdErrors);
            return false;
        }
        
        private async Task<TEntity> GetFullModel(TCmd cmd)
        {
            var queryable = DbContext.GetDbSet<TEntity>().Where(cmd.GetEntity());
            return await queryable.FirstOrDefaultAsync();
        }
        
        public async Task<CmdResult> EditDependent(TCmd cmd, int changedBy)
        {
            ChangedBy = changedBy;
            
            try
            {
                var existingEntity = await GetFullModel(cmd);
                var newEntity = cmd.Construct();

                if (StateIsValid(newEntity))
                {
                    DbContext.Remove(existingEntity);
                    
                    var audit = 
                        new Audit("Update", 
                            existingEntity.GetType().Name, 
                            existingEntity.GetPrimaryKey().ToString(), 
                            JsonSerializer.Serialize(existingEntity),
                            ChangedBy);

                    await DbContext.Audit.AddAsync(audit);
                    await DbContext.AddAsync(newEntity, CancellationToken.None);
                    await DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                CmdResult.AddIssue(ex.InnerException != null
                    ? $"{ex.Message}. [Inner exception] {ex.InnerException.Message}"
                    : ex.Message);
            }

            return CmdResult;
        }
    }
}