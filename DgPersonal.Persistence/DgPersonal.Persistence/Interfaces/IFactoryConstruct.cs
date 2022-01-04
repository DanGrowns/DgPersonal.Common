namespace DgPersonal.Persistence.Interfaces
{
    public interface IFactoryConstruct<TEntity>
    {
        TEntity Construct();
    }
}