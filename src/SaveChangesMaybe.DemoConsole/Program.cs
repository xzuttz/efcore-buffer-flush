// See https://aka.ms/new-console-template for more information

using Microsoft.Data.Sqlite;
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

    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();

    var options = new DbContextOptionsBuilder<SchoolContext>()
        .UseSqlite(connection)
        .Options;

    var dbCtx = new SchoolContext(options);

    dbCtx.Database.EnsureDeleted();
    dbCtx.Database.EnsureCreated();

    services.AddSingleton<SchoolContext>(dbCtx);

    services.AddSaveChangesMaybeServiceFactory();
}