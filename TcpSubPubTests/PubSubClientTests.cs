using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Commom.Libraries.Interfaces;
using Commom.Libraries.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace TcpSubPubTests
{
    /// <summary>
    /// Test suite for PubSubClient functionality.
    /// </summary>
    public class PubSubClientTests
    {
        /// <summary>
        /// Tests if the PubSubClient can successfully connect to a valid endpoint.
        /// </summary>
        [Fact]
        public async Task ConnectAsync_WithValidEndPoint_ShouldConnectSuccessfully()
        {
            // Define the server address and port
            string address = "127.0.0.1";
            int port = 12345;

            // Mock a logger for the PubSubServer
            var mockServerLogger = new Mock<ILogger<PubSubServer>>();

            // Start a PubSubServer instance
            using var server = new PubSubServer(address, port, mockServerLogger.Object);
            server.Start();

            try
            {
                // Mock a logger for PubSubClient
                var mockLogger = new Mock<ILogger<PubSubClient>>();

                // Create a PubSubClient instance and connect to the server
                using var pubSubClient = new PubSubClient(mockLogger.Object);
                await pubSubClient.ConnectAsync(address, port);

                // Assert that the client is connected successfully
                Assert.True(pubSubClient.IsConnected());
            }
            finally
            {
                // Ensure the server is stopped after the test
                server.Stop();
            }
        }

        /// <summary>
        /// Tests if the PubSubClient throws an exception when trying to connect to an invalid endpoint.
        /// </summary>
        [Fact]
        public async Task ConnectAsync_WithInvalidEndPoint_ShouldThrowException()
        {
            // Define server and client details
            string validAddress = "127.0.0.1";
            string invalidAddress = "807.0.0.1"; // Deliberately incorrect IP address
            int port = 12345;

            // Mock a logger for the PubSubServer
            var mockServerLogger = new Mock<ILogger<PubSubServer>>();

            // Start a PubSubServer instance with valid settings
            using var server = new PubSubServer(validAddress, port, mockServerLogger.Object);
            server.Start();

            try
            {
                // Mock a logger for PubSubClient
                var mockLogger = new Mock<ILogger<PubSubClient>>();

                // Create a PubSubClient instance
                using var client = new PubSubClient(mockLogger.Object);

                // Assert that an exception is thrown when connecting to an invalid endpoint
                await Assert.ThrowsAsync<FormatException>(() => client.ConnectAsync(invalidAddress, port));
            }
            finally
            {
                // Stop the server (ensure isolation of tests)
                server.Stop();
            }
        }

        /// <summary>
        /// Tests the ability of a PubSubClient to successfully connect to a running PubSubServer instance.
        /// Verifies that the connection is established by asserting the connected status of the client.
        /// The test involves starting a PubSubServer, creating a PubSubClient, and then connecting the client to the server.
        /// </summary>
        [Fact]
        public async Task ConnectAsync_PubSubServerInstance_ShouldConnectSuccessfully()
        {
            // Initialize server and client details
            string address = "127.0.0.1";
            int port = 12345;

            // Start a PubSubServer instance
            var server = new PubSubServer(address, port);
            server.Start();

            try
            {
                // Mock a logger for PubSubClient
                var mockLogger = new Mock<ILogger<PubSubClient>>();

                // Create and connect a PubSubClient instance
                using var client = new PubSubClient(mockLogger.Object);
                await client.ConnectAsync(address, port);

                // Assert that the client is connected successfully
                Assert.True(client.IsConnected());
            }
            finally
            {
                // Stop the server (ensure isolation of tests)
                server.Stop();
            }
        }

        /// <summary>
        /// Tests that the PubSubClient can successfully publish a message.
        /// Verifies that the message ID returned upon publishing is not null, 
        /// indicating successful message publication.
        /// </summary>
        [Fact]
        public async Task PublishAsync_WithValidMessage_ShouldPublishSuccessfully()
        {
            // Define the server address and port
            string address = "127.0.0.1";
            int port = 12345;

            // Mock a logger for the PubSubServer
            var mockServerLogger = new Mock<ILogger<PubSubServer>>();

            // Start a PubSubServer instance
            var pubSubServer = new PubSubServer(address, port, mockServerLogger.Object);
            pubSubServer.Start();

            try
            {
                // Mock a logger for PubSubClient
                var mockLogger = new Mock<ILogger<PubSubClient>>();

                // Create a PubSubClient instance and connect to the server
                using var pubSubClient = new PubSubClient(mockLogger.Object);
                await pubSubClient.ConnectAsync(address, port);

                // Publish a message
                var message = "Test message";
                var messageId = await pubSubClient.PublishAsync(message);

                // Assert that the message ID is not null
                Assert.NotNull(messageId);
            }
            finally
            {
                // Ensure the server is stopped after the test
                pubSubServer.Stop();
            }
        }

        /// <summary>
        /// Tests if the PubSubClient can subscribe successfully to a valid message.
        /// </summary>
        [Fact]
        public async Task SubscribeAsync_WithValidMessage_ShouldSubscribeSuccessfully()
        {
            string address = "127.0.0.1";
            int port = 5001;

            var mockServerLogger = new Mock<ILogger<PubSubServer>>();
            Mock<ILogger<PubSubClient>> mockSubscriberLogger = new Mock<ILogger<PubSubClient>>(), mockPublisherLogger = new Mock<ILogger<PubSubClient>>();

            // Initialize the PubSub server.
            var server = new PubSubServer(address, port, mockServerLogger.Object);
            server.Start();

            try
            {
                // Initialize a PubSubClient for subscribing and connecting to the server.
                var subscriber = new PubSubClient(mockSubscriberLogger.Object);

                // Connect the subscriber to the server.
                await subscriber.ConnectAsync(address, port);

                // Initialize a PubSubClient for publishing messages.
                var publisher = new PubSubClient(mockPublisherLogger.Object);

                // Connect the publisher to the server.
                await publisher.ConnectAsync(server);

                var messageId = await publisher.PublishAsync($"Test message");
                await subscriber.SubscribeAsync();

                mockSubscriberLogger.VerifyLog(
                logger => logger.LogInformation(It.Is<string>(x => x.Contains("Received message"))));
            }
            finally
            {
                // Stop the server when done.
                server.Stop();
            }
        }

    }
}
