using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Commom.Libraries.Models;
using Microsoft.Extensions.Logging;

internal class Program
{
    public static async Task Main(string[] args)
    {
        string address = "127.0.0.1";
        int port = 5001;

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        // Initialize the PubSub server.
        using var server = new PubSubServer(address, port, loggerFactory.CreateLogger<PubSubServer>());
        server.Start();

        try
        {
            // Initialize a PubSubClient for subscribing and connecting to the server.
            using var subscriber = new PubSubClient(loggerFactory.CreateLogger<PubSubClient>());

            // Connect the subscriber to the server.
            await subscriber.ConnectAsync(address, port);

            // Initialize a PubSubClient for publishing messages.
            using var publisher = new PubSubClient(loggerFactory.CreateLogger<PubSubClient>());

            // Connect the publisher to the server.
            await publisher.ConnectAsync(server);

            // Publish and subscribe in a loop for 10 rounds.
            for (int i = 0; i < 10; i++)
            {
                await publisher.PublishAsync($"Round #{i} - Timestamp: {DateTimeOffset.UtcNow}");
                await subscriber.SubscribeAsync();
                Task.Delay(1000).Wait();
            }
        }
        finally
        {
            // Stop the server when done.
            server.Stop();
        }
    }
}
