using GFlow.Application.Ports;
using GFlow.Application.Services;
using GFlow.Infrastructure.Persistance;
using GFlow.Infrastructure.Persistance.Repositories;
using GFlow.Infrastructure.Security;
using GFlow.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;



/// <summary>
/// Entry point for the GFlow API application.
/// Configures services, authentication, database connection, and request pipeline.
/// </summary>

// 1. Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURATION VARIABLES ---
var appUrls = Environment.GetEnvironmentVariable("APP_URLS") ?? "http://localhost:5249";
// CRITICAL: Fail if DB connection string is missing in production-like environments or use a safe default for local dev if absolutely necessary.
// For security, remove the hardcoded password fallback or ensure it matches a local dev container only.
// Here we enforce checking the environment variable for sensitive data or throw/log warning.
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") 
    ?? throw new InvalidOperationException("DB_CONNECTION environment variable is invalid or missing.");

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
    ?? throw new InvalidOperationException("JWT_KEY environment variable is missing.");
    
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
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
        Description = "Paste JWT token here"
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

// Register Token Provider
builder.Services.AddScoped<ITokenProvider>(sp => 
    new JwtTokenProvider(jwtKey, jwtIssuer));

// Register Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGeoLocationService, IpApiGeoLocationService>();
builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();

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

// Order is critical!
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// Automatic Database Migration/Creation
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try 
    {
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"CRITICAL ERROR: Could not connect to the database.");
        Console.WriteLine($"Error details: {ex.Message}");
        Console.WriteLine($"Please ensure PostgreSQL is running at {connectionString.Split(';')[0]}"); // Crude hint from connection string
        Console.ResetColor();
        Environment.Exit(1);
    }
}

app.Run();