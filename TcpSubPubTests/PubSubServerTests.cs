using System;
using System.Net;
using System.Net.Sockets;
using Commom.Libraries.Interfaces;
using Commom.Libraries.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace TcpSubPubTests
{
    /// <summary>
    /// Contains tests for validating the lifecycle operations of PubSubServer.
    /// </summary>
    public class PubSubServerTests
    {
        /// <summary>
        /// Tests the lifecycle of a PubSubServer instance, including start, stop, and disposal operations.
        /// Ensures that each operation logs the appropriate message, verifying the server's behavior.
        /// </summary>
        [Fact]
        public void Start_Stop_ServerLifecycle()
        {
            // Arrange
            var address = "127.0.0.1";
            var port = 5001;

            // Create a mock logger to capture log messages.
            var mockLogger = new Mock<ILogger<PubSubServer>>();

            // Create a PubSubServer instance for testing.
            using var server = new PubSubServer(address, port, mockLogger.Object);

            // Act
            server.Start();
            server.Stop();
            server.Dispose();

            // Assert

            // Verify that Start method logs the server start message.
            mockLogger.VerifyLog(
                logger => logger.LogInformation("Server started and listening on {0}:{1}", It.IsAny<IPAddress>(), It.IsAny<int>()));

            // Verify that Stop method logs the server stop message.
            mockLogger.VerifyLog(
                logger => logger.LogInformation("Server stopped"));

            // Verify that Dispose method logs the server dispose message.
            mockLogger.VerifyLog(
                logger => logger.LogInformation("Server disposed"));
        }
    }
}