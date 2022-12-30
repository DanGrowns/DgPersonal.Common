using System;
using System.Data;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IDbConnector
    {
        IDbConnection GetConnection(string key = "Database");
        T GetConnectionAsync<T>(string key = "Database") 
            where T : class, IDbConnection, IAsyncDisposable;
    }
}