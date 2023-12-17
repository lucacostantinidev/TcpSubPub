using System.Net;
using System.Net.Sockets;

namespace Commom.Libraries.Interfaces
{
    /// <summary>
    /// Defines the interface for a Publish-Subscribe server.
    /// </summary>
    public interface IPubSubServer : IDisposable
    {
        /// <summary>
        /// Retrieves a TCP client connected to the server.
        /// </summary>
        /// <returns>A <see cref="TcpClient"/> object representing the connected client.</returns>
        Task<TcpClient> GetClient();

        /// <summary>
        /// Retrieves the network endpoint the server is listening on.
        /// </summary>
        /// <returns>An <see cref="IPEndPoint"/> representing the network endpoint.</returns>
        IPEndPoint GetIPEndPoint();

        /// <summary>
        /// Starts the server, allowing it to accept incoming connections.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the server, preventing any new connections and shutting down existing ones.
        /// </summary>
        void Stop();
    }
}
