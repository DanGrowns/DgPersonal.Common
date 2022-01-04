using System;
using DgPersonal.Persistence.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DgPersonal.Persistence.Classes
{
    public class SqlServerConnector : IDbConnector
    {
        private string ConnectionString { get; set; }
        private IConfiguration Configuration { get; }
        
        public SqlServerConnector()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.development.json", false, true);

            Configuration = builder.Build();
        }
        
        public SqlServerConnector(IConfiguration configuration) 
            => Configuration = configuration;
        
        public string GetConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString) == false)
                return ConnectionString;

            ConnectionString = Configuration.GetValue<string>("Sql");
            return ConnectionString;
        }
    }
}