using Microsoft.EntityFrameworkCore;
using Serilog;
using ShiftLogger.API.Data;
using ShiftLogger.API.Services;

var loggingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
Directory.CreateDirectory(loggingDirectory);
string filePath = Path.Combine(loggingDirectory, "shift-logger-.txt");

string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
    path: filePath, 
    rollingInterval: RollingInterval.Day, 
    outputTemplate: outputTemplate)
    .CreateLogger();


try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<ShiftLoggerDbContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

    builder.Services.AddScoped<IShiftService, ShiftService>();
    builder.Services.AddScoped<IWorkerService, WorkerService>();

    
    var app = builder.Build();


    //Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
