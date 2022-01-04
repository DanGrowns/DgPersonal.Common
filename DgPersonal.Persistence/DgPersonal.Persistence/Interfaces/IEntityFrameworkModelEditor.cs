using System.Threading.Tasks;
using TinyCqrs.Classes;
using TinyFluentValidator.Interfaces;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IEntityFrameworkModelEditor<TEntity, TCmd>
        where TEntity : class, IStateChange<TCmd>, IValidationEntity<TEntity>, IEntity
        where TCmd : IPersistenceCmd<TEntity>, IFindEntity<TEntity>
    {
        TEntity TrackedEntity { get; }
        Task<CmdResult> Edit(TCmd cmd, int changedBy);
    }
}