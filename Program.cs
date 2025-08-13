using FORO_UTTN_API.Models;
using FORO_UTTN_API.Utils;
using Microsoft.AspNetCore.Builder;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de MongoDB
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
    new MongoClient(mongoConnectionString)
);

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("foro_uttn")
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Registrar MongoService (esto es necesario para el LoginController)
builder.Services.AddSingleton<MongoService>();

var app = builder.Build();

app.UseCors("AllowAll");

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
