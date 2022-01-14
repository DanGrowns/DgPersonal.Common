namespace DgPersonal.Persistence.Interfaces
{
    public interface IDbConnector
    {
        string GetConnection(string key = "Sql");
    }
}