using Microsoft.AspNetCore.Builder;
using MongoDB.Driver;

// Removed 'public' modifier from the local function as it is not valid for local functions.
// Also ensured the function is invoked to avoid the CS8321 warning.

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigureServices(builder.Services); // Ensure the function is invoked.

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

void ConfigureServices(IServiceCollection services)
{
    var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");

    services.AddSingleton<IMongoClient, MongoClient>(sp =>
        new MongoClient(mongoConnectionString)
    );

    services.AddSingleton(sp =>
        sp.GetRequiredService<IMongoClient>().GetDatabase("foro_uttn")
    );

    // Otros servicios...
}
