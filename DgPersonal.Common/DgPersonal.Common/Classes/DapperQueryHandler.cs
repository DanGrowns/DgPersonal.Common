using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using DgPersonal.Common.Attributes;
using DgPersonal.Common.Interfaces;
using Microsoft.Data.SqlClient;

namespace DgPersonal.Common.Classes
{
    public class DapperQueryHandler : IDapperQueryHandler
    {
        private string ConnectionString { get; }
        protected SqlMapper.GridReader Reader { get; set; }

        public DapperQueryHandler(IDbConnector dbConnector)
            => ConnectionString = dbConnector.GetConnection();
        
        private DbConnection NewDbConnection()
            => new SqlConnection(ConnectionString);

        private static string GetStoredProcedure<TOutput>()
        {
            var name = typeof(TOutput).Name;
            var defaultSp = $"dbo.{name}_Get";
            
            var attribute = typeof(TOutput).GetCustomAttribute<DbProcedure>();
            if (attribute != null) 
                return string.IsNullOrEmpty(attribute.SpName) ? defaultSp : attribute.SpName;
            
            return defaultSp;
        }
        
        public async Task<TOutput> GetFirstOrDefault<TOutput>(object sqlParameters = null)
        {
            await using var connection = NewDbConnection();
            
            var result = 
                await connection.QueryFirstOrDefaultAsync<TOutput>(
                    GetStoredProcedure<TOutput>(), 
                    sqlParameters,
                    commandType: CommandType.StoredProcedure);
    
            return result;
        }
        
        public async Task<List<TOutput>> GetList<TOutput>(object sqlParameters = null)
        {
            await using var connection = NewDbConnection();
            
            var enumerable = 
                await connection.QueryAsync<TOutput>(
                    GetStoredProcedure<TOutput>(), 
                    sqlParameters,
                    commandType: CommandType.StoredProcedure);
    
            return enumerable.ToList();
        }
        
        /// <summary>
        /// Suggestion: When using GetMultiple use the query handler as a base class so any transformations of data
        /// can happen as part of a single testable unit, rather than being placed in a controller or somewhere tied to a view.
        /// </summary>
        protected async Task<TOutput> GetMultiple<TOutput>(string storedProcedureName, Func<TOutput> mapFunc,
            object sqlParameters = null)
        {
            await using var connection = NewDbConnection();
            
            Reader = 
                await connection.QueryMultipleAsync(
                    storedProcedureName, 
                    sqlParameters,
                    commandType: CommandType.StoredProcedure);

            return mapFunc();
        }
    }
}