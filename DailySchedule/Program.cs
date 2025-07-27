using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DailySchedule.Middleware; // Pastikan authMiddleware ada di namespace ini

var builder = WebApplication.CreateBuilder(args);

// Ambil nilai dari environment Railway
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

// Validasi variabel lingkungan
if (string.IsNullOrEmpty(jwtKey))
    throw new Exception("JWT_KEY belum diatur di environment variable Railway");

if (string.IsNullOrEmpty(connectionString))
    throw new Exception("DEFAULT_CONNECTION belum diatur di environment variable Railway");

// Konfigurasi appsettings
builder.Configuration["Jwt:Key"] = jwtKey;
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// Tambahkan service JWT Authentication
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

// Tentukan port dari Railway atau fallback ke 3000
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
app.Urls.Add($"http://*:{port}");

// Aktifkan Swagger (akses di /swagger)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DailySchedule API V1");
    c.RoutePrefix = "swagger";
});

// Middleware
app.UseAuthentication();
app.UseAuthorization();

// Middleware custom kecuali untuk swagger dan favicon
app.UseWhen(
    context =>
        !context.Request.Path.StartsWithSegments("/swagger") &&
        !context.Request.Path.StartsWithSegments("/favicon.ico"),
    appBuilder =>
    {
        appBuilder.UseMiddleware<authMiddleware>();
    }
);

// Routing Controller
app.MapControllers();

app.Run();
