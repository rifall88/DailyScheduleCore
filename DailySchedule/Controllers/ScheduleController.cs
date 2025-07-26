using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using DailySchedule.Models;

namespace DailySchedule.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly ScheduleModel _scheduleModel;

        public ScheduleController(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                 ?? throw new Exception("Connection string 'DefaultConnection' not found.");
            _scheduleModel = new ScheduleModel(connectionString);
        }

        [HttpPost]
        public IActionResult CreateSchedule([FromBody] ScheduleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Data tidak valid.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var result = _scheduleModel.Create(userId, request.Date!.Value, request.Time!.Value, request.Title!, request.Description!);
                return StatusCode(201, new
                {
                    message = "Jadwal berhasil dibuat.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating schedule: " + ex.Message);
                return StatusCode(500, new { message = "Gagal membuat jadwal." });
            }
        }

        [HttpGet]
        public IActionResult GetAllSchedules()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var table = _scheduleModel.FindAll(userId);

                var list = new List<Dictionary<string, object>>();

                foreach (DataRow row in table.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        dict[col.ColumnName] = row[col] is DBNull ? null! : row[col];
                    }
                    list.Add(dict);
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting schedules: " + ex.Message);
                return StatusCode(500, new { message = "Gagal mengambil jadwal." });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSchedule(int id, [FromBody] ScheduleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Data tidak valid.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var updated = _scheduleModel.Update(id, userId,
                    request.Date!.Value,
                    request.Time!.Value,
                    request.Title!,
                    request.Description!);

                return Ok(new
                {
                    message = "Jadwal berhasil diperbarui",
                    data = updated
                });
            }
            catch (PostgresException pgEx) when (pgEx.SqlState.StartsWith("P"))
            {
                return BadRequest(new { message = "Kesalahan database saat memperbarui jadwal." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating schedule: " + ex.Message);
                return StatusCode(500, new { message = "Gagal memperbarui jadwal." });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSchedule(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                _scheduleModel.Delete(id, userId);
                return Ok(new { message = "Jadwal berhasil dihapus." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting schedule: " + ex.Message);
                return StatusCode(500, new { message = "Gagal menghapus jadwal." });
            }
        }

        public class ScheduleRequest
        {
            [Required(ErrorMessage = "Judul tidak boleh kosong.")]
            public string? Title { get; set; }

            [Required(ErrorMessage = "Deskripsi tidak boleh kosong.")]
            public string? Description { get; set; }

            [Required(ErrorMessage = "Tanggal wajib diisi.")]
            public DateTime? Date { get; set; }

            [Required(ErrorMessage = "Waktu wajib diisi.")]
            public TimeSpan? Time { get; set; }
        }
    }
}
