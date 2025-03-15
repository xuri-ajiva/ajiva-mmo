// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

using System.Net;
using System.Net.Sockets;

namespace Riptide.Transports.Udp
{
    /// <summary>The kind of socket to create.</summary>
    public enum SocketMode
    {
        /// <summary>Dual-mode. Works with both IPv4 and IPv6.</summary>
        Both,
        /// <summary>IPv4 only mode.</summary>
        PV4_ONLY,
        /// <summary>IPv6 only mode.</summary>
        PV6_ONLY
    }

    /// <summary>Provides base send &#38; receive functionality for <see cref="UdpServer"/> and <see cref="UdpClient"/>.</summary>
    public abstract class UdpPeer
    {
        /// <inheritdoc cref="IPeer.Disconnected"/>
        public event EventHandler<DisconnectedEventArgs>? Disconnected;

        /// <summary>The default size used for the socket's send and receive buffers.</summary>
        protected const int DefaultSocketBufferSize = 1024 * 1024; // 1MB
        /// <summary>The minimum size that may be used for the socket's send and receive buffers.</summary>
        private const int MinSocketBufferSize = 256 * 1024; // 256KB
        /// <summary>How long to wait for a packet, in microseconds.</summary>
        private const int ReceivePollingTime = 500000; // 0.5 seconds

        /// <summary>Whether to create an IPv4 only, IPv6 only, or dual-mode socket.</summary>
        protected readonly SocketMode Mode;
        /// <summary>The size to use for the socket's send and receive buffers.</summary>
        private readonly int _socketBufferSize;
        /// <summary>The array that incoming data is received into.</summary>
        private readonly byte[] _receivedData;
        /// <summary>The socket to use for sending and receiving.</summary>
        private Socket _socket = null!;
        /// <summary>Whether or not the transport is running.</summary>
        private bool _isRunning;
        /// <summary>A reusable endpoint.</summary>
        private EndPoint _remoteEndPoint = null!;

        /// <summary>Initializes the transport.</summary>
        /// <param name="mode">Whether to create an IPv4 only, IPv6 only, or dual-mode socket.</param>
        /// <param name="socketBufferSize">How big the socket's send and receive buffers should be.</param>
        protected UdpPeer(SocketMode mode, int socketBufferSize)
        {
            if (socketBufferSize < MinSocketBufferSize)
                throw new ArgumentOutOfRangeException(nameof(socketBufferSize), $"The minimum socket buffer size is {MinSocketBufferSize}!");

            this.Mode = mode;
            this._socketBufferSize = socketBufferSize;
            _receivedData = new byte[Message.MaxSize];
        }

        /// <inheritdoc cref="IPeer.Poll"/>
        public void Poll()
        {
            Receive();
        }

        /// <summary>Opens the socket and starts the transport.</summary>
        /// <param name="listenAddress">The IP address to bind the socket to, if any.</param>
        /// <param name="port">The port to bind the socket to.</param>
        protected void OpenSocket(IPAddress? listenAddress = null, ushort port = 0)
        {
            if (_isRunning)
                CloseSocket();

            if (Mode == SocketMode.PV4_ONLY)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            else if (Mode == SocketMode.PV6_ONLY)
                _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp) { DualMode = false };
            else
                _socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            IPAddress any = _socket.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any;
            _socket.SendBufferSize = _socketBufferSize;
            _socket.ReceiveBufferSize = _socketBufferSize;
            _socket.Bind(new IPEndPoint(listenAddress ?? any, port));
            _remoteEndPoint = new IPEndPoint(any, 0);

            _isRunning = true;
        }

        /// <summary>Closes the socket and stops the transport.</summary>
        protected void CloseSocket()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _socket.Close();
        }

        /// <summary>Polls the socket and checks if any data was received.</summary>
        private void Receive()
        {
            if (!_isRunning)
                return;

            bool tryReceiveMore = true;
            while (tryReceiveMore)
            {
                int byteCount = 0;
                try
                {
                    if (_socket.Available > 0 && _socket.Poll(ReceivePollingTime, SelectMode.SelectRead))
                        byteCount = _socket.ReceiveFrom(_receivedData, SocketFlags.None, ref _remoteEndPoint);
                    else
                        tryReceiveMore = false;
                }
                catch (SocketException ex)
                {
                    tryReceiveMore = false;
                    switch (ex.SocketErrorCode)
                    {
                        case SocketError.Interrupted:
                        case SocketError.NotSocket:
                            _isRunning = false;
                            break;
                        case SocketError.ConnectionReset:
                        case SocketError.MessageSize:
                        case SocketError.TimedOut:
                        default:
                            break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    tryReceiveMore = false;
                    _isRunning = false;
                }
                catch (NullReferenceException)
                {
                    tryReceiveMore = false;
                    _isRunning = false;
                }

                if (byteCount > 0)
                    OnDataReceived(_receivedData, byteCount, (IPEndPoint)_remoteEndPoint);
            }
        }

        /// <summary>Sends data to a given endpoint.</summary>
        /// <param name="dataBuffer">The array containing the data.</param>
        /// <param name="numBytes">The number of bytes in the array which should be sent.</param>
        /// <param name="toEndPoint">The endpoint to send the data to.</param>
        internal void Send(byte[] dataBuffer, int numBytes, IPEndPoint toEndPoint)
        {
            try
            {
                if (_isRunning)
                    _socket.SendTo(dataBuffer, numBytes, SocketFlags.None, toEndPoint);
            }
            catch(SocketException)
            {
                // May want to consider triggering a disconnect here (perhaps depending on the type
                // of SocketException)? Timeout should catch disconnections, but disconnecting
                // explicitly might be better...
            }
        }

        /// <summary>Handles received data.</summary>
        /// <param name="dataBuffer">A byte array containing the received data.</param>
        /// <param name="amount">The number of bytes in <paramref name="dataBuffer"/> used by the received data.</param>
        /// <param name="fromEndPoint">The endpoint from which the data was received.</param>
        protected abstract void OnDataReceived(byte[] dataBuffer, int amount, IPEndPoint fromEndPoint);

        /// <summary>Invokes the <see cref="Disconnected"/> event.</summary>
        /// <param name="connection">The closed connection.</param>
        /// <param name="reason">The reason for the disconnection.</param>
        protected virtual void OnDisconnected(Connection connection, DisconnectReason reason)
        {
            Disconnected?.Invoke(this, new DisconnectedEventArgs(connection, reason));
        }
    }
}
