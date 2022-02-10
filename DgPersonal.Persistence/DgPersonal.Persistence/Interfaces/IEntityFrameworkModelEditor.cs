using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TinyCqrs.Classes;
using TinyFluentValidator.Interfaces;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IEntityFrameworkModelEditor<TEntity, in TCmd> 
        where TEntity : class, IStateChange<TCmd>, IValidationEntity<TEntity>, IEntity
    {
        TEntity TrackedEntity { get; }
        
        /// <summary>
        /// All public types supporting ICollection or IReadOnlyCollection in TEntity will be included as navigations.
        /// </summary>
        Task<CmdResult> Edit<TEditCmd>(TEditCmd cmd, int changedBy) where TEditCmd : IFindEntity<TEntity>;

        Task<CmdResult> Edit<TEditCmd>(TEditCmd cmd, int changedBy, List<string> navigationIncludes) where TEditCmd : IFindEntity<TEntity>;

        Task<CmdResult> Edit(TCmd cmd, int changedBy, Expression<Func<TEntity, bool>> findEntityExpression, List<string> navigationIncludes = null);
    }
}