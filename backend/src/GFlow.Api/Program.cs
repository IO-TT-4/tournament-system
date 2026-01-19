using GFlow.Application.Ports;
using GFlow.Application.Services;
using GFlow.Infrastructure.Persistance.Migrations;
using GFlow.Infrastructure.Persistance.Repositories;
using GFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.WebHost.UseUrls("http://localhost:5249", "http://192.168.0.20:5249");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tournament.db"));

builder.Services.AddScoped<IUserRepository, UserRepositoryPostgres>();
builder.Services.AddScoped<ITournamentRepository, TournamentRepositoryPostgres>();

builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "TajemniczyKlucz_Minimum_32_Znaki_123!";

builder.Services.AddScoped<ITokenProvider>(sp => 
    new JwtTokenProvider(jwtKey));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITournamentService, TournamentService>();

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated(); // To stworzy plik tournament.db i tabele
}

app.Run();
