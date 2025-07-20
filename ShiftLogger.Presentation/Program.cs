using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ShiftLogger.Presentation.Clients;


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
    })
    .Build();
