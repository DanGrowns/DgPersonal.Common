using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DgPersonal.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using TinyCqrs.Classes;
using TinyFluentValidator.Classes;
using TinyFluentValidator.Interfaces;

namespace DgPersonal.Persistence.Classes
{
    public class EntityFrameworkModelEditor<TEntity, TCmd>  : IEntityFrameworkModelEditor<TEntity, TCmd>
        where TEntity : class, IStateChange<TCmd>, IValidationEntity<TEntity>, IEntity
    {
        private IDbContext DbContext { get; }
        private IValidator<TEntity> Validator { get; }
        private int ChangedBy { get; set; }
        
        private CmdResult CmdResult { get; }
        private List<string> NavigationIncludes { get; set; }
        private Expression<Func<TEntity, bool>> GetEntityExpression { get; set; }

        public EntityFrameworkModelEditor(IDbContext dbContext, IValidator<TEntity> validator)
        {
            DbContext = dbContext;
            Validator = validator;
            
            CmdResult = new CmdResult($"Edit {typeof(TEntity).Name.SplitPascalCaseToString()}");
        }
        
        public TEntity TrackedEntity { get; private set; }

        private async Task<TEntity> GetFullModel(TCmd cmd)
        {
            var queryable = DbContext.GetDbSet<TEntity>().Where(GetEntityExpression);
            
            queryable = NavigationIncludes.Aggregate(queryable, (current, include) 
                => current.Include(include));

            return await queryable.FirstOrDefaultAsync() 
                        ?? (TEntity) Activator.CreateInstance(typeof(TEntity), cmd);
        }
        
        protected virtual Task<IReadOnlyList<string>> ExternalValidations()
            => Task.FromResult((IReadOnlyList<string>) new List<string>());
        
        private async Task CheckStateIsValid(TEntity dbModel)
        {
            var internalState = dbModel.StateIsValid(Validator);
            var externalState = await ExternalValidations();

            var errors = new List<string>();
            errors.AddRange(internalState);
            errors.AddRange(externalState);
            
            if (errors.Count < 1) 
                return;
            
            var cmdErrors = 
                internalState.Select(x => new CmdIssue(CmdResult.SourceName, x));
                
            CmdResult.Issues.AddRange(cmdErrors);
        }
        
        private async Task UpdateExisting(TCmd cmd, TEntity dbModel)
        {
            var audit = 
                new Audit("Update", 
                    dbModel.GetType().Name, 
                    dbModel.GetPrimaryKey().ToString(), 
                    JsonSerializer.Serialize(dbModel),
                    ChangedBy);
            
            dbModel.SetStateFromDto(cmd);
            await CheckStateIsValid(dbModel);
            
            if (CmdResult.Issues.Count > 0)
                return;

            DbContext.Update(dbModel);
            DbContext.Audit.Add(audit);
            
            await DbContext.SaveChangesAsync();

            TrackedEntity = dbModel;
        }

        private async Task CreateNew(TEntity dbModel)
        {
            await CheckStateIsValid(dbModel);
            
            if (CmdResult.Issues.Count > 0)
                return;

            DbContext.Add(dbModel);
            await DbContext.SaveChangesAsync();
            
            var audit = 
                new Audit("Add", 
                    dbModel.GetType().Name, 
                    dbModel.GetPrimaryKey().ToString(), 
                    "",
                    ChangedBy);
            
            DbContext.Audit.Add(audit);
            await DbContext.SaveChangesAsync();

            TrackedEntity = dbModel;
        }

        private void BuildNavigationIncludes()
        {
            NavigationIncludes = new List<string>();
            
            foreach (var prop in typeof(TEntity).GetProperties(BindingFlags.Public))
            {
                if (prop.GetType().TypeSupportsInterface(typeof(ICollection<>))
                    || prop.GetType().TypeSupportsInterface(typeof(IReadOnlyCollection<>)))
                {
                    NavigationIncludes.Add(prop.Name);
                }
            }
        }
        
        private async Task<CmdResult> EditModel(TCmd cmd, int changedBy)
        {
            ChangedBy = changedBy;
            
            try
            {
                var dbModel = await GetFullModel(cmd);

                if (dbModel.Exists())
                    await UpdateExisting(cmd, dbModel);
                else
                    await CreateNew(dbModel);
            }
            catch (Exception ex)
            {
                CmdResult.AddIssue(ex.InnerException != null
                    ? $"{ex.Message}. [Inner exception] {ex.InnerException.Message}"
                    : ex.Message);
            }

            return CmdResult;
        }
        
        private static ArgumentException CommandDoesNotMatchException<TEditCmd>() 
            => throw new ArgumentException($"{typeof(TEditCmd).Name} must match ${typeof(TCmd).Name}");
        
        public async Task<CmdResult> Edit<TEditCmd>(TEditCmd editCmd, int changedBy) where TEditCmd : IFindEntity<TEntity>
        {
            if (editCmd is not TCmd cmd)
                throw CommandDoesNotMatchException<TEditCmd>();
            
            BuildNavigationIncludes();
            GetEntityExpression = editCmd.GetEntity();
            
            return await EditModel(cmd, changedBy);
        }

        public async Task<CmdResult> Edit<TEditCmd>(TEditCmd editCmd, int changedBy, List<string> navigationIncludes) where TEditCmd : IFindEntity<TEntity>
        {
            if (editCmd is not TCmd cmd)
                throw CommandDoesNotMatchException<TEditCmd>();
            
            NavigationIncludes = navigationIncludes ?? new List<string>();
            GetEntityExpression = editCmd.GetEntity();
            
            return await EditModel(cmd, changedBy);
        }

        public async Task<CmdResult> Edit(TCmd cmd, int changedBy, Expression<Func<TEntity, bool>> findEntityExpression)
        {
            BuildNavigationIncludes();
            GetEntityExpression = findEntityExpression;

            return await EditModel(cmd, changedBy);
        }

        public async Task<CmdResult> Edit(TCmd cmd, int changedBy, Expression<Func<TEntity, bool>> findEntityExpression, List<string> navigationIncludes)
        {
            NavigationIncludes = navigationIncludes ?? new List<string>();
            GetEntityExpression = findEntityExpression;

            return await EditModel(cmd, changedBy);
        }
    }
}