using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace DailySchedule.Models
{
    public class userModel
    {
        public class UserModel
        {
            private readonly string _connectionString;

            public UserModel(string connectionString)
            {
                _connectionString = connectionString;
            }

            public DataRow? FindByEmail(string email)
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand("SELECT * FROM users WHERE email = @Email", conn);
                cmd.Parameters.Add("@Email", NpgsqlTypes.NpgsqlDbType.Varchar).Value = email;

                using var adapter = new NpgsqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                return table.Rows.Count > 0 ? table.Rows[0] : null;
            }

            public void Create(string name, string email, string hashedPassword)
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    "INSERT INTO users (name, email, password) VALUES (@Name, @Email, @Password)", conn
                );
                cmd.Parameters.Add("@Name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = name;
                cmd.Parameters.Add("@Email", NpgsqlTypes.NpgsqlDbType.Varchar).Value = email;
                cmd.Parameters.Add("@Password", NpgsqlTypes.NpgsqlDbType.Varchar).Value = hashedPassword;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
