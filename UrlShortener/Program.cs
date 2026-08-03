using Microsoft.EntityFrameworkCore;

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();
