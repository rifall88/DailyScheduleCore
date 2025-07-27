using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Ambil JWT Key dan Connection String dari environment variable
builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY");
builder.Configuration["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

// Validasi JWT Key wajib ada
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new Exception("JWT key tidak ditemukan di environment variable!");

// Konfigurasi JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Aktifkan Swagger di semua environment
app.UseSwagger();
app.UseSwaggerUI();

// KECUALIKAN MIDDLEWARE AUTH UNTUK SWAGGER
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/swagger"),
    appBuilder => appBuilder.UseMiddleware<authMiddleware>()
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// FIX: Ambil PORT dari environment, fallback ke 3000 jika tidak ada
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
app.Run($"http://0.0.0.0:{port}");
