using DC.Application.Mapping;
using domainInterfaces = DC.Domain.Interfaces;
using DC.Infrastructure.Data;
using DC.Infrastructure.Logging;
using DC.Infrastructure.Repositories;
using DC.Presentation.Controllers;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<DepthChartDbContext>(options =>
{
    // Use an in-memory database
    options.UseInMemoryDatabase("InMemoryDb");
});

// Register repositories
builder.Services.AddScoped<domainInterfaces.ISportRepository, SportRepository>();
builder.Services.AddScoped<domainInterfaces.ITeamRepository, TeamRepository>();
builder.Services.AddScoped<domainInterfaces.IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<domainInterfaces.IPositionRepository, PositionRepository>();
builder.Services.AddScoped<domainInterfaces.IOrderRepository, OrderRepository>();

builder.Services.AddScoped<domainInterfaces.ILogger<SportController>, AppLogger<SportController>>();
builder.Services.AddScoped<domainInterfaces.ILogger<TeamController>, AppLogger<TeamController>>();
builder.Services.AddScoped<domainInterfaces.ILogger<PlayerController>, AppLogger<PlayerController>>();
builder.Services.AddScoped<domainInterfaces.ILogger<PositionController>, AppLogger<PositionController>>();
builder.Services.AddScoped<domainInterfaces.ILogger<OrderController>, AppLogger<OrderController>>();

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
