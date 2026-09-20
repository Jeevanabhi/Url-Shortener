
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<RabbitMqConsumer>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<ClickService>(); 
builder.Services.AddDbContext<UrlShorten>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 36))));
var host = builder.Build();
host.Run();
