using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// DI, Logging with Serilog, and use AppSettings

namespace BetterConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("Application starting");
            GreetingService svc;
            using (var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => { services.AddTransient<IGreetingService, GreetingService>(); })
                .UseSerilog()
                .Build())
            {
                svc = ActivatorUtilities.CreateInstance<GreetingService>(host.Services);
            }

            svc.Run();
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder
                // .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
