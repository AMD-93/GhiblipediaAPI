using GhiblipediaAPI.Data;
using GhiblipediaAPI.Services;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using Npgsql;
using System;
using System.Data;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IMovieRepository, MovieRepository>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Environment variable CONNECTION_STRING is not set.");
}

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(connectionString));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://*:{port}");
    Console.WriteLine($"Listening on port: {port}"); 
}
else
{    
    app.Urls.Add("http://*:8080");
    Console.WriteLine("PORT environment variable not set, falling back to default 8080.");
}

if (!app.Environment.IsProduction() || app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
