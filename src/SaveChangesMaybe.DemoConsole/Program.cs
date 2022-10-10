// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaveChangesMaybe;
using SaveChangesMaybe.DemoConsole;
using SaveChangesMaybe.DemoConsole.Models;
using Serilog;

CreateLogger();
var host = CreateHost();
await host.RunAsync();
return 0;

static IHost CreateHost()
{
    return new HostBuilder()
        .ConfigureServices(ConfigureApplicationServices)
        .UseSerilog()
        .Build();
}

static void CreateLogger()
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
}

static void ConfigureApplicationServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddHostedService<SaveChangesMaybeWorker>();

    services.AddDbContext<SchoolContext>(optionsBuilder =>
    {
        optionsBuilder.UseInMemoryDatabase(databaseName: "SchoolDbInMemory");
    });

    services.AddSaveChangesMaybeServiceFactory();
}