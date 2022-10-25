using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IDapperQueryHandler
    {
        SqlMapper.GridReader Reader { get; }
        
        Task<TOutput> GetFirstOrDefault<TOutput>(object sqlParameters = null, string storedProcedureName = null);
        Task<List<TOutput>> GetList<TOutput>(object sqlParameters = null, string storedProcedureName = null);
        Task<TOutput> GetMultiple<TOutput>(string storedProcedureName, Func<TOutput> mapFunc,
            object sqlParameters = null);
    }
}