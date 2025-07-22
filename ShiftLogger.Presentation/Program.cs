using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ShiftLogger.Presentation.Clients;
using ShiftLogger.Presentation.UI;


using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((logCtx, log) => log.AddSerilog())
    .ConfigureServices((ctx, services) =>
    {
        var baseUrl = ctx.Configuration["ApiSettings:BaseUrl"]!;

        services.AddHttpClient<IApiClient>("Api", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept
            .Add(new("application/json"));
        });
        services.AddTransient<IApiClient, ApiClient>();
        services.AddTransient<IShiftLoggerUI, ShiftLoggerUI>();
    })
    .Build();

var userInterface = host.Services.GetRequiredService<IShiftLoggerUI>();
await userInterface.RunAsync(cts.Token);
