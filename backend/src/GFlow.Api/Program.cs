using GFlow.Application.Ports;
using GFlow.Application.Services;
using GFlow.Infrastructure.Persistance;
using GFlow.Infrastructure.Persistance.Repositories;
using GFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

// 1. Ładowanie pliku .env
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// --- KONFIGURACJA ZMIENNYCH ---
var appUrls = Environment.GetEnvironmentVariable("APP_URLS") ?? "http://localhost:5249";
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? "Data Source=tournament.db";
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "DefaultSuperSecretKey123_MustBeLong";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "GFlowApp";

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.WebHost.UseUrls(appUrls.Split(';'));

// --- AUTHENTICATION (JWT) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- SWAGGER WITH AUTH ---
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "GFlow API", Version = "v1" });
    
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Wklej sam token JWT"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            new string[]{}
        }
    });
});

// --- DATABASE ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- DI REGISTRATION ---
builder.Services.AddScoped<IUserRepository, UserRepositoryPostgres>();
builder.Services.AddScoped<ITournamentRepository, TournamentRepositoryPostgres>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();

// Przekazujemy klucz i issuer do providera tokenów
builder.Services.AddScoped<ITokenProvider>(sp => 
    new JwtTokenProvider(jwtKey, jwtIssuer));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITournamentService, TournamentService>();

var app = builder.Build();

// --- MIDDLEWARE PIPELINE ---
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Kolejność jest kluczowa!
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// Automatyczna migracja/tworzenie bazy
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();