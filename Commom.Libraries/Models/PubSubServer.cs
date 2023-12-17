using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Commom.Libraries.Interfaces;

namespace Commom.Libraries.Models
{
    public class PubSubServer : IPubSubServer
    {
        private readonly IPEndPoint _endPoint;
        private readonly TcpListener _tcpListener;
        private readonly ILogger<PubSubServer> _logger;

        public PubSubServer(string address, int port, ILogger<PubSubServer> logger = null)
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            _tcpListener = new TcpListener(_endPoint);
            _logger = logger;
        }

        /// <summary>
        /// Starts the server, allowing it to accept client connections.
        /// </summary>
        public void Start()
        {
            _tcpListener.Start();
            _logger?.LogInformation("Server started and listening on {0}:{1}", _endPoint.Address, _endPoint.Port);
        }

        /// <summary>
        /// Stops the server, preventing it from accepting new client connections.
        /// </summary>
        public void Stop()
        {
            _tcpListener.Stop();
            _logger?.LogInformation("Server stopped");
        }

        /// <summary>
        /// Gets the server's endpoint (IP address and port).
        /// </summary>
        /// <returns>The server's endpoint.</returns>
        public IPEndPoint GetIPEndPoint()
        {
            return _endPoint;
        }

        /// <summary>
        /// Accepts a client connection asynchronously.
        /// </summary>
        /// <returns>The connected TcpClient.</returns>
        public async Task<TcpClient> GetClient()
        {
            var client = await _tcpListener.AcceptTcpClientAsync();
            _logger?.LogInformation("Accepted client connection from {0}:{1}", ((IPEndPoint)client.Client.RemoteEndPoint).Address, ((IPEndPoint)client.Client.RemoteEndPoint).Port);
            return client;
        }

        /// <summary>
        /// Disposes of the server and associated resources.
        /// </summary>
        public void Dispose()
        {
            _tcpListener?.Stop();
            _logger?.LogInformation("Server disposed");
        }
    }
}
