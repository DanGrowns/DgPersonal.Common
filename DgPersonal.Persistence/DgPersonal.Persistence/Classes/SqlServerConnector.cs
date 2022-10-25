﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DgPersonal.Persistence.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DgPersonal.Persistence.Classes
{
    public class SqlServerConnector : IDbConnector
    {
        private List<Tuple<string, string>> ConnectionStrings { get; set; }
        private IConfiguration Configuration { get; }

        private SqlServerConnector() => ConnectionStrings = new List<Tuple<string, string>>();
        public SqlServerConnector(string fileName = "appsettings.development.json") : this()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(fileName, false, true);

            Configuration = builder.Build();
        }
        
        public SqlServerConnector(IConfiguration configuration) : this()
            => Configuration = configuration;
        
        private string GetConnectionString(string key = "Sql")
        {
            var existingEntry = ConnectionStrings.FirstOrDefault(x => x.Item1 == key);
            if (existingEntry != null)
                return existingEntry.Item2;

            var connectionString = Configuration.GetValue<string>(key);
            
            ConnectionStrings.Add(new Tuple<string, string>(key, connectionString));
            
            return connectionString;
        }

        public IDbConnection GetConnection(string key = "Sql")
        {
            var connectionString = GetConnectionString(key);
            return new SqlConnection(connectionString);
        }
        
        public T GetConnectionAsync<T>(string key = "Sql") where T : class, IDbConnection, IAsyncDisposable
        {
            var connectionString = GetConnectionString(key);
            var sql = new SqlConnection(connectionString);
            return sql as T;
        }
    }
}