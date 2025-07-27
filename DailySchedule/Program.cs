using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Ambil nilai JWT_KEY dan DEFAULT_CONNECTION dari Environment Variable
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

if (string.IsNullOrEmpty(jwtKey))
    throw new Exception("JWT_KEY belum diatur di environment variable Railway");

if (string.IsNullOrEmpty(connectionString))
    throw new Exception("DEFAULT_CONNECTION belum diatur di environment variable Railway");

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

// Tentukan PORT yang digunakan Railway (gunakan default 3000 jika tidak ada)
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
app.Urls.Add($"http://*:{port}");

// Aktifkan Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Jalankan authMiddleware kecuali untuk swagger dan favicon
app.UseWhen(
    context =>
        !context.Request.Path.StartsWithSegments("/swagger") &&
        !context.Request.Path.StartsWithSegments("/favicon.ico"),
    appBuilder => appBuilder.UseMiddleware<authMiddleware>()
);

// Middleware urutan penting
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
