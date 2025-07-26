using DailySchedule.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static DailySchedule.Models.userModel;

namespace DailySchedule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserModel _userModel;
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            _userModel = new UserModel(_configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Connection string 'DefaultConnection' tidak ditemukan."));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Nama wajib diisi." });

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email wajib diisi." });

            if (!request.Email.Contains("@") || !request.Email.Contains("."))
                return BadRequest(new { message = "Format email tidak valid." });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Password wajib diisi." });

            if (request.Password.Length < 6)
                return BadRequest(new { message = "Password minimal 6 karakter." });

            try
            {
                var existingUser = _userModel.FindByEmail(request.Email);
                if (existingUser != null)
                {
                    return Conflict(new { message = "Email sudah terdaftar." });
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                _userModel.Create(request.Name, request.Email, hashedPassword);

                return Created("", new
                {
                    status = true,
                    message = "Registrasi berhasil.",
                    data = new
                    {
                        name = request.Name,
                        email = request.Email
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Register error: " + ex.Message);
                return StatusCode(500, new { message = "Terjadi kesalahan saat registrasi." });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email wajib diisi." });

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Password wajib diisi." });

            try
            {
                var userRow = _userModel.FindByEmail(request.Email);
                if (userRow == null)
                {
                    return Unauthorized(new { message = "Email tidak ditemukan." });
                }

                var hashedPassword = userRow["password"]?.ToString();
                if (string.IsNullOrEmpty(hashedPassword))
                {
                    return StatusCode(500, new { message = "Data pengguna tidak valid." });
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, hashedPassword);

                if (!isValid)
                {
                    return Unauthorized(new { message = "Password salah." });
                }

                var token = GenerateJwtToken(userRow);

                return Ok(new
                {
                    status = true,
                    message = "Login berhasil.",
                    data = new
                    {
                        id = userRow["id"],
                        name = userRow["name"],
                        email = userRow["email"],
                        access_token = token
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login error: " + ex.Message);
                return StatusCode(500, new { message = "Terjadi kesalahan saat login." });
            }
        }

        private string GenerateJwtToken(System.Data.DataRow user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = _configuration["Jwt:Key"]
                ?? throw new Exception("JWT key tidak ditemukan di konfigurasi.");

            var key = Encoding.UTF8.GetBytes(keyString);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user["id"]?.ToString() ?? ""),
                    new Claim(ClaimTypes.Name, user["name"]?.ToString() ?? ""),
                    new Claim(ClaimTypes.Email, user["email"]?.ToString() ?? "")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public class RegisterRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
