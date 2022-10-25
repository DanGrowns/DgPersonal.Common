using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using DgPersonal.Persistence.Attributes;
using DgPersonal.Persistence.Interfaces;
using Microsoft.Data.SqlClient;

namespace DgPersonal.Persistence.Classes
{
    public class DapperQueryHandler : IDapperQueryHandler
    {
        private IDbConnector DbConnector { get; }

        public DapperQueryHandler(IDbConnector dbConnector)
            => DbConnector = dbConnector;
        
        public SqlMapper.GridReader Reader { get; private set; }
        
        private static string GetStoredProcedure<TOutput>()
        {
            var name = typeof(TOutput).Name;
            var defaultSp = $"dbo.{name}_Get";
            
            var attribute = typeof(TOutput).GetCustomAttribute<DbProcedure>();
            if (attribute != null) 
                return string.IsNullOrEmpty(attribute.SpName) ? defaultSp : attribute.SpName;
            
            return defaultSp;
        }
        
        public async Task<TOutput> GetFirstOrDefault<TOutput>(object sqlParameters = null, string storedProcedureName = null)
        {
            var spName = string.IsNullOrEmpty(storedProcedureName)
                ? GetStoredProcedure<TOutput>()
                : storedProcedureName;
            
            await using var connection = DbConnector.GetConnectionAsync<SqlConnection>();
            
            var result = 
                await connection.QueryFirstOrDefaultAsync<TOutput>(
                    spName, 
                    sqlParameters,
                    commandType: CommandType.StoredProcedure);
    
            return result;
        }
        
        public async Task<List<TOutput>> GetList<TOutput>(object sqlParameters = null, string storedProcedureName = null)
        {
            var spName = string.IsNullOrEmpty(storedProcedureName)
                ? GetStoredProcedure<TOutput>()
                : storedProcedureName;
            
            await using var connection = DbConnector.GetConnectionAsync<SqlConnection>();
            
            var enumerable = 
                await connection.QueryAsync<TOutput>(
                    spName, 
                    sqlParameters,
                    commandType: CommandType.StoredProcedure);
    
            return enumerable.ToList();
        }
        
        /// <summary>
        /// Suggestion: When using GetMultiple use the query handler as a base class so any transformations of data
        /// can happen as part of a single testable unit, rather than being placed in a controller or somewhere tied to a view.
        /// </summary>
        public async Task<TOutput> GetMultiple<TOutput>(string storedProcedureName, Func<TOutput> mapFunc,
            object sqlParameters = null)
        {
            await using var connection = DbConnector.GetConnectionAsync<SqlConnection>();
            
            Reader = 
                await connection.QueryMultipleAsync(
                    storedProcedureName, 
                    sqlParameters,
                    commandType: CommandType.StoredProcedure);

            return mapFunc();
        }
    }
}