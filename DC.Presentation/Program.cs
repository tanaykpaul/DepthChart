using DC.Application.Mapping;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using DC.Infrastructure.Logging;
using DC.Infrastructure.Repositories;
using DC.Infrastructure.Services;
using DC.Presentation.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Configure JSON serializer to handle reference loops by preserving object references
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve property names
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Register AutoMapper with the MappingProfile
builder.Services.AddAutoMapper(typeof(MappingProfile));

#region Database Management
builder.Services.AddDbContext<DepthChartDbContext>(options =>
{
    // Use an in-memory database
    options.UseInMemoryDatabase("InMemoryDb");
});
// For SQL Server database usages
// 1. Comment out the previus line for "InMemoryDatabase"
// 2. Check your ConnectionString in the appsettings.json file
// 2. Run two commands from your Package Manager Console
//      add-migration InitialMigration -Project DC.Infrastructure -StartupProject DC.Presentation
//      update-database -Project DC.Infrastructure -StartupProject DC.Presentation
// 3. Uncomment the following line
/*builder.Services.AddDbContext<DepthChartDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});*/
#endregion

// Register repositories
builder.Services.AddScoped<IAppLogger, AppLogger>();
builder.Services.AddScoped<ISportRepository, SportRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register the unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
