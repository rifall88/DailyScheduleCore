using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Ambil JWT Key dan Connection String dari environment variable
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

if (string.IsNullOrEmpty(jwtKey))
    throw new Exception("JWT_KEY belum diatur di environment variable Railway");

if (string.IsNullOrEmpty(connectionString))
    throw new Exception("DEFAULT_CONNECTION belum diatur di environment variable Railway");

builder.Configuration["Jwt:Key"] = jwtKey;
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

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

// Set port Railway (gunakan PORT dari env var atau default 3000)
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
app.Urls.Add($"http://*:{port}");

// Aktifkan Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Kecualikan middleware custom auth untuk swagger
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/swagger"),
    appBuilder => appBuilder.UseMiddleware<authMiddleware>()
);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
