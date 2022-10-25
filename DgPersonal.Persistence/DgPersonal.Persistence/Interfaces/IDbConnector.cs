using System;
using System.Data;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IDbConnector
    {
        IDbConnection GetConnection(string key = "Sql");
        T GetConnectionAsync<T>(string key = "Sql") 
            where T : class, IDbConnection, IAsyncDisposable;
    }
}