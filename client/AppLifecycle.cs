namespace Client;

using System.Threading;
using System.Threading.Tasks;
using ChangeFeed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// This hosted service allows for startup and shutdown activities related to the application itself.
/// </summary>
internal class AppLifecycle : IHostedService
{
    private readonly IConfig config;
    private readonly IChangeFeed changeFeed;
    private readonly CancellationTokenSource cts = new();
    private readonly ILogger<AppLifecycle> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLifecycle"/> class.
    /// </summary>
    /// <param name="config">The configuration for this application.</param>
    /// <param name="changeFeed">The change feed.</param>
    /// <param name="logger">The logger.</param>
    public AppLifecycle(IConfig config, IChangeFeed changeFeed, ILogger<AppLifecycle> logger)
    {
        this.config = config;
        this.changeFeed = changeFeed;
        this.logger = logger;
    }

    /// <summary>
    /// This method should contain all startup activities for the application.
    /// </summary>
    /// <param name="cancellationToken">A token that can be cancelled to abort startup.</param>
    /// <returns>A Task that is complete when the method is done.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // validate the configuration
        this.config.Validate();

        // add an event handler
        this.changeFeed.OnNotifiedAsync += (object sender, string payload, CancellationToken cancellationToken) =>
        {
            this.logger.LogInformation("received notification: '{payload}'.", payload);
            return Task.CompletedTask;
        };

        // listen for changes
        await this.changeFeed.ListenAsync(this.cts.Token);

        // notify about a change
        await this.changeFeed.NotifyAsync("evict:customers", this.cts.Token);
        this.logger.LogInformation("sent notification.");
    }

    /// <summary>
    /// This method should contain all shutdown activities for the application.
    /// </summary>
    /// <param name="cancellationToken">A token that can be cancelled to abort startup.</param>
    /// <returns>A Task that is complete when the method is done.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.cts.Cancel();
        this.cts.Dispose();
        return Task.CompletedTask;
    }
}