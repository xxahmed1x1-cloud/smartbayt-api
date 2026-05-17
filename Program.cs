using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartBayt.Data;
using SmartBayt.Helpers;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ─────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── JWT ──────────────────────────────────────────────────
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

// ─── CORS ─────────────────────────────────────────────────
builder.Services.AddCors(opt =>
    opt.AddPolicy("Frontend", p => p
        .WithOrigins(
    "http://localhost:5173",
    "http://localhost:3000",
    "http://localhost:8080",
    builder.Configuration["Frontend:Url"] ?? "http://localhost:5173"
)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    )
);

// ─── Controllers ──────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ─── Auto-migrate ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ─── Middleware ───────────────────────────────────────────
app.UseStaticFiles();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
