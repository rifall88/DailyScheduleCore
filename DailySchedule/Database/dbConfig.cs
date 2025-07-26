using Npgsql;
using Microsoft.Extensions.Configuration;

namespace DailySchedule.Database
{
        public class dbConfig
        {
            private readonly IConfiguration _configuration;

            public dbConfig(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public NpgsqlConnection GetConnection()
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                return new NpgsqlConnection(connectionString);
            }
        }
    }