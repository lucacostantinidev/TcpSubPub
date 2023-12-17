using Commom.Libraries.Models;
using System.Net;

namespace Commom.Libraries.Interfaces
{
    /// <summary>
    /// Represents a client for a publish-subscribe system.
    /// </summary>
    public interface IPubSubClient : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Connects to a server using an IPEndPoint.
        /// </summary>
        /// <param name="endPoint">The endpoint of the server to connect to.</param>
        Task ConnectAsync(IPEndPoint endPoint);

        /// <summary>
        /// Connects to a specified server instance.
        /// </summary>
        /// <param name="server">The PubSubServer instance to connect to.</param>
        Task ConnectAsync(PubSubServer server);

        /// <summary>
        /// Connects to a server using an address and port.
        /// </summary>
        /// <param name="address">The IP address of the server.</param>
        /// <param name="port">The port number of the server.</param>
        Task ConnectAsync(string address, int port);

        /// <summary>
        /// Publishes a message to the server.
        /// </summary>
        /// <param name="message">The message to be published.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the GUID of the published message.</returns>
        Task<Guid> PublishAsync(string message);

        /// <summary>
        /// Subscribes to messages from the server.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer for message subscriptions, with a default size of 1024.</param>
        Task SubscribeAsync(int bufferSize = 1024);
    }

}