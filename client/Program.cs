namespace Client;

using ChangeFeed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// The application.
/// </summary>
internal class Program
{
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    internal static void Main(string[] args)
    {
        // create the host
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureServices((hostContext, services) =>
            {
                // add hosted services
                services.AddHostedService<AppLifecycle>();

                // add services
                services.AddSingleton<IConfig, Config>();
                services.AddEventHubChangeFeed<IConfig>();
            });

        // build and run
        host.Build().Run();
    }
}