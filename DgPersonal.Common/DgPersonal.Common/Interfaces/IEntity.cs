namespace DgPersonal.Common.Interfaces
{
    public interface IEntity
    {
        object GetPrimaryKey();
        bool Exists();
    }
}