using System.Data;
using Npgsql;

namespace DailySchedule.Models
{
        public class ScheduleModel
        {
            private readonly string _connectionString;

            public ScheduleModel(string connectionString)
            {
                _connectionString = connectionString;
            }

            public Dictionary<string, object> Create(int userId, DateTime date, TimeSpan time, string title, string description)
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(@"
                INSERT INTO schedules (user_id, date, time, title, description) 
                VALUES (@UserId, @Date, @Time, @Title, @Description)
                RETURNING id, user_id, title, description, date, time;", conn);

                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.Parameters.AddWithValue("@Time", time);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Description", description);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var result = new Dictionary<string, object>
                    {
                        ["id"] = reader["id"],
                        ["user_id"] = reader["user_id"],
                        ["title"] = reader["title"],
                        ["description"] = reader["description"],
                        ["date"] = reader["date"],
                        ["time"] = reader["time"]
                    };
                    return result;
                }

                throw new Exception("Gagal menyimpan dan mengambil data.");
            }

            public DataTable FindAll(int userId)
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(@"
                 SELECT * FROM schedules WHERE user_id = @UserId", conn);

                cmd.Parameters.AddWithValue("@UserId", userId);

                using var adapter = new NpgsqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                return table;
            }

            public object Update(int id, int userId, DateTime date, TimeSpan time, string title, string description)
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(@"
            UPDATE schedules 
            SET date = @Date, time = @Time, title = @Title, description = @Description 
            WHERE id = @Id AND user_id = @UserId", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.Parameters.AddWithValue("@Time", time);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Description", description);

                cmd.ExecuteNonQuery();

                return new
                {
                    id = id,
                    user_id = userId,
                    title = title,
                    description = description,
                    date = date,
                    time = time
                };
            }

            public void Delete(int id, int userId)
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(@"
                DELETE FROM schedules 
                WHERE id = @Id AND user_id = @UserId", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@UserId", userId);

                cmd.ExecuteNonQuery();
            }
        }
    }
