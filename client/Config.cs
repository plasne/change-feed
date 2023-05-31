namespace Client;

using System;
using System.Linq;
using ChangeFeed;
using Microsoft.Extensions.Configuration;

/// <summary>
/// This class is used to configure this application.
/// </summary>
internal class Config : IConfig, IEventHubChangeFeedConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Config"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public Config(IConfiguration config)
    {
        this.CHANGEFEED_CONNSTRING = config.GetValue<string>("CHANGEFEED_CONNSTRING");
        this.CHANGEFEED_CONSUMER_GROUPS = config.GetValue<string>("CHANGEFEED_CONSUMER_GROUPS")
            .Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
    }

    /// <inheritdoc />
    public string CHANGEFEED_CONNSTRING { get; }

    /// <inheritdoc />
    public string[] CHANGEFEED_CONSUMER_GROUPS { get; }

    /// <inheritdoc />
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(this.CHANGEFEED_CONNSTRING))
        {
            throw new Exception("CHANGEFEED_CONNSTRING is required.");
        }

        if (this.CHANGEFEED_CONSUMER_GROUPS.Length < 1)
        {
            throw new Exception("CHANGEFEED_CONSUMER_GROUPS is required.");
        }
    }
}