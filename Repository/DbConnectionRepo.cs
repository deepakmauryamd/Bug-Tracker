using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace BugTracker.Repository
{
    public class DbConnectionRepo
    {
        private readonly IConfiguration _config;

        public DbConnectionRepo(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new MySqlConnection(_config.GetConnectionString("bugtrackerdbString"));
            }
        }
    }
}