using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<UrlService>();
// var connectionString = builder.Configuration.GetConnectionString("Connectstring");
// builder.Services.AddDbContext<UrlShorten>(options =>
// {
//     options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
// });
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UrlShorten>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 36))));

var redisHost = builder.Configuration["REDIS_HOST"]
    ?? throw new InvalidOperationException("REDIS_HOST is not configured.");
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect($"{redisHost}:6379"));
builder.Services.AddSingleton<RabbitMqPublisher>();   
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();
