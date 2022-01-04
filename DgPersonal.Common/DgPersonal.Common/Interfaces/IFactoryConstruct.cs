namespace DgPersonal.Common.Interfaces
{
    public interface IFactoryConstruct<TEntity>
    {
        TEntity Construct();
    }
}