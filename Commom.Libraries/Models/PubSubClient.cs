using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Commom.Libraries.Interfaces;

namespace Commom.Libraries.Models
{
    /// <summary>
    /// Represents a client for publishing and subscribing to messages over TCP.
    /// </summary>
    public class PubSubClient : IPubSubClient
    {
        private readonly ILogger<PubSubClient> _logger;
        private readonly TcpClient _tcpClient;
        private NetworkStream _networkStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="PubSubClient"/> class.
        /// </summary>
        /// <param name="logger">Optional logger for logging messages.</param>
        public PubSubClient(ILogger<PubSubClient> logger = null)
        {
            _logger = logger;
            _tcpClient = new TcpClient();
        }

        /// <summary>
        /// Connects to an endpoint using its IP address and port.
        /// </summary>
        /// <param name="endPoint">The endpoint to connect to.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task ConnectAsync(IPEndPoint endPoint)
        {
            try
            {
                // Attempt to establish a connection to the specified endpoint.
                await _tcpClient.ConnectAsync(endPoint);

                _logger?.LogInformation($"Client connected to IP address {endPoint.Address}:{endPoint.Port}");

                // Get the network stream to send and receive data.
                _networkStream = _tcpClient.GetStream();
            }
            catch (Exception ex)
            {
                // Handle and log any connection errors.
                HandleError("Error connecting to endpoint", ex);
                throw;
            }
        }

        /// <summary>
        /// Connects to an endpoint using an IP address and port number.
        /// </summary>
        /// <param name="address">The IP address of the endpoint.</param>
        /// <param name="port">The port number of the endpoint.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task ConnectAsync(string address, int port)
        {
            try
            {
                var endPoint = new IPEndPoint(IPAddress.Parse(address), port);
                // Use the previous method to establish the connection.
                await ConnectAsync(endPoint);
            }
            catch (Exception ex)
            {
                HandleError($"Error connecting to {address}:{port}", ex);
                throw;
            }
        }

        /// <summary>
        /// Connects to a server using a PubSubServer instance.
        /// </summary>
        /// <param name="server">The PubSubServer instance to connect to.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task ConnectAsync(PubSubServer server)
        {
            try
            {
                var endPoint = server.GetIPEndPoint();
                // Get the client from the server and connect using the endpoint.
                var client = await server.GetClient();
                await ConnectAsync(endPoint);
                _networkStream = client.GetStream();
            }
            catch (Exception ex)
            {
                HandleError("Error connecting to server", ex);
                throw;
            }
        }

        /// <summary>
        /// Publishes a message to the connected endpoint.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task<Guid> PublishAsync(string message)
        {
            try
            {
                var id = Guid.NewGuid();
                var enrichedMessage = EnrichMessage(id, message);
                var messageBytes = Encoding.UTF8.GetBytes(enrichedMessage);

                // Send the message over the network stream.
                await _networkStream.WriteAsync(messageBytes);

                _logger?.LogInformation($"Sent message with id {id}");

                return id;
            }
            catch (Exception ex)
            {
                HandleError("Error publishing message", ex);
                throw;
            }
        }

        /// <summary>
        /// Subscribes to incoming messages from the connected endpoint.
        /// </summary>
        /// <param name="bufferSize">The size of the receive buffer.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task SubscribeAsync(int bufferSize = 1024)
        {
            try
            {
                var buffer = new byte[bufferSize];
                int received = await _networkStream.ReadAsync(buffer);

                // Convert the received bytes to a string message.
                var message = Encoding.UTF8.GetString(buffer, 0, received);

                _logger?.LogInformation($"Received message '{message}'");
            }
            catch (Exception ex)
            {
                HandleError("Error subscribing to messages", ex);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously disposes of the client and associated resources.
        /// </summary>
        /// <returns>An asynchronous task.</returns>
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_networkStream != null)
                {
                    _networkStream.Close();
                    await _networkStream.DisposeAsync();
                }
                _tcpClient.Close();
                _tcpClient.Dispose();
            }
            catch (Exception ex)
            {
                HandleError("Error disposing client", ex);
                throw;
            }
        }

        /// <summary>
        /// Synchronously disposes of the client.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_networkStream != null)
                {
                    _networkStream.Close();
                    _networkStream.Dispose();
                }
                _tcpClient.Close();
                _tcpClient.Dispose();
            }
            catch (Exception ex)
            {
                HandleError("Error disposing client", ex);
                throw;
            }
        }

        public bool IsConnected() => _tcpClient != null && _networkStream != null;

        // Enriches a message with a GUID identifier.
        private string EnrichMessage(Guid guid, string message) => $"Id: {guid}, Original Message: {message}";

        // Handles and logs errors with a specific error message.
        private void HandleError(string errorMessage, Exception ex)
        {
            _logger?.LogError($"{errorMessage}: {ex.Message}");
        }
    }
}
