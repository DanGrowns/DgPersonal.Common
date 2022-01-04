namespace DgPersonal.Persistence.Interfaces
{
    public interface IEntity
    {
        object GetPrimaryKey();
        bool Exists();
    }
}