using System.Threading.Tasks;
using TinyCqrs.Classes;
using TinyFluentValidator.Interfaces;

namespace DgPersonal.Common.Interfaces
{
    public interface IEntityFrameworkDependentModelEditor<TEntity, TCmd>
        where TEntity : class, IStateChange<TCmd>, IValidationEntity<TEntity>, IEntity
        where TCmd :  IFindEntity<TEntity>, IFactoryConstruct<TEntity>
    {
        Task<CmdResult> EditDependent(TCmd cmd, int changedBy);
    }
}