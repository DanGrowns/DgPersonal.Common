using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DgPersonal.Extensions.General.Classes;
using DgPersonal.Persistence.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TinyCqrs.Classes;
using TinyFluentValidator.Interfaces;

namespace DgPersonal.Persistence.Classes
{
    public class EntityFrameworkModelEditor<TEntity, TCmd>  : IEntityFrameworkModelEditor<TEntity, TCmd>
        where TEntity : class, IStateChange<TCmd>, IValidationTarget<TEntity>, IEntity
    {
        private IDbContext DbContext { get; }
        private IValidator<TEntity> Validator { get; }
        private int ChangedBy { get; set; }
        
        private TEntity OriginalState { get; set; }
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
        public bool IsNewEntry { get; private set; }

        private async Task<TEntity> GetFullModel(TCmd cmd)
        {
            var queryable = DbContext.GetDbSet<TEntity>().Where(GetEntityExpression);
            
            queryable = NavigationIncludes.Aggregate(queryable, (current, include) 
                => current.Include(include));

            var result = await queryable.FirstOrDefaultAsync();
            
            if (result == null)
            {
                var hasCmdConstructor = typeof(TEntity).GetConstructor(new []{typeof(TCmd)}) != null;
                return hasCmdConstructor
                    ? (TEntity) Activator.CreateInstance(typeof(TEntity), cmd)
                    : Activator.CreateInstance<TEntity>();
                
            }
            
            return result;
        }
        
        protected virtual Task<IReadOnlyList<string>> ExternalValidations()
            => Task.FromResult((IReadOnlyList<string>) new List<string>());
        
        private async Task CheckStateIsValid(TEntity dbModel)
        {
            var internalState = dbModel.IsValid(Validator);
            var externalState = await ExternalValidations();

            var errors = new List<string>();
            errors.AddRange(internalState.Errors);
            errors.AddRange(externalState);
            
            if (errors.Count < 1) 
                return;
            
            CmdResult.Issues.AddRange(errors.Select(x => new CmdIssue(CmdResult.SourceName, x)));
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
            
            var properties = typeof(TEntity).GetProperties();
            foreach (var prop in properties)
            {
                var ignorable = prop.GetCustomAttributes<NotMappedAttribute>().Any();
                if (ignorable)
                    continue;

                if (prop.PropertyType.TypeSupportsInterfaces(new []{typeof(ICollection<>), typeof(IReadOnlyCollection<>)}))
                    NavigationIncludes.Add(prop.Name);

                if (prop.PropertyType.Name == typeof(IReadOnlyCollection<>).Name
                    || prop.PropertyType.Name == typeof(IReadOnlyList<>).Name)
                    NavigationIncludes.Add(prop.Name);
            }
        }
        
        private async Task<CmdResult> EditModel(TCmd cmd, int changedBy)
        {
            ChangedBy = changedBy;

            try
            {
                var dbModel = await GetFullModel(cmd);

                IsNewEntry = dbModel.Exists() == false;
                if (IsNewEntry)
                {
                    await CreateNew(dbModel);
                }
                else
                {
                    var hasEmptyConstructor = typeof(TEntity).GetConstructor(Type.EmptyTypes) != null;
                    if (hasEmptyConstructor)
                        OriginalState = dbModel.Adapt<TEntity>();
                    
                    await UpdateExisting(cmd, dbModel);
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
        
        private static ArgumentException CommandDoesNotMatchException<TEditCmd>() 
            => throw new ArgumentException($"{typeof(TEditCmd).Name} must match ${typeof(TCmd).Name}");
        
        public async Task<CmdResult> Edit<TEditCmd>(TEditCmd editCmd, int changedBy) 
            where TEditCmd : IFindEntity<TEntity>
        {
            if (editCmd is not TCmd cmd)
                throw CommandDoesNotMatchException<TEditCmd>();
            
            BuildNavigationIncludes();
            GetEntityExpression = editCmd.GetEntity();
            
            return await EditModel(cmd, changedBy);
        }

        public async Task<CmdResult> Edit<TEditCmd>(TEditCmd editCmd, int changedBy, List<string> navigationIncludes) 
            where TEditCmd : IFindEntity<TEntity>
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

        public TEntity GetOriginalState()
        {
            if (IsNewEntry)
                return null;

            if (OriginalState == null)
                throw new ArgumentException("A parameterless constructor is required on the database model to obtain the original state.");

            return OriginalState;
        }
    }
}