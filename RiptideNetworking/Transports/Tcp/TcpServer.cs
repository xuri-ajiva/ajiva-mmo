// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

using System.Net;
using System.Net.Sockets;

namespace Riptide.Transports.Tcp
{
    /// <summary>A server which can accept connections from <see cref="TcpClient"/>s.</summary>
    public class TcpServer : TcpPeer, IServer
    {
        /// <inheritdoc/>
        public event EventHandler<ConnectedEventArgs>? Connected;

        /// <inheritdoc/>
        public event EventHandler<DataReceivedEventArgs>? DataReceived;

        /// <inheritdoc/>
        public ushort Port { get; private set; }

        /// <summary>The maximum number of pending connections to allow at any given time.</summary>
        public int MaxPendingConnections { get; private set; } = 5;

        /// <summary>Whether or not the server is running.</summary>
        private bool _isRunning = false;

        /// <summary>The currently open connections, accessible by their endpoints.</summary>
        private Dictionary<IPEndPoint, TcpConnection> _connections = null!;

        /// <summary>Connections that have been closed and need to be removed from <see cref="_connections"/>.</summary>
        private readonly List<IPEndPoint> _closedConnections = [];

        /// <summary>The IP address to bind the socket to.</summary>
        private readonly IPAddress _listenAddress;

        /// <inheritdoc/>
        public TcpServer(int socketBufferSize = DefaultSocketBufferSize) : this(IPAddress.IPv6Any, socketBufferSize)
        {
        }

        /// <summary>Initializes the transport, binding the socket to a specific IP address.</summary>
        /// <param name="listenAddress">The IP address to bind the socket to.</param>
        /// <param name="socketBufferSize">How big the socket's send and receive buffers should be.</param>
        public TcpServer(IPAddress listenAddress, int socketBufferSize = DefaultSocketBufferSize) : base(
            socketBufferSize)
        {
            this._listenAddress = listenAddress;
        }

        /// <inheritdoc/>
        public void Start(ushort port)
        {
            Port = port;
            _connections = new Dictionary<IPEndPoint, TcpConnection>();

            StartListening(port);
        }

        /// <summary>Starts listening for connections on the given port.</summary>
        /// <param name="port">The port to listen on.</param>
        private void StartListening(ushort port)
        {
            if (_isRunning)
                StopListening();

            IPEndPoint localEndPoint = new IPEndPoint(_listenAddress, port);
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                SendBufferSize = SocketBufferSize,
                ReceiveBufferSize = SocketBufferSize,
                NoDelay = true,
            };
            Socket.Bind(localEndPoint);
            Socket.Listen(MaxPendingConnections);

            _isRunning = true;
        }

        /// <inheritdoc/>
        public void Poll()
        {
            if (!_isRunning)
                return;

            Accept();
            foreach (TcpConnection connection in _connections.Values)
                connection.Receive();

            foreach (IPEndPoint endPoint in _closedConnections)
                _connections.Remove(endPoint);

            _closedConnections.Clear();
        }

        /// <summary>Accepts any pending connections.</summary>
        private void Accept()
        {
            if (Socket.Poll(0, SelectMode.SelectRead))
            {
                Socket acceptedSocket = Socket.Accept();
                if (acceptedSocket.RemoteEndPoint is IPEndPoint fromEndPoint
                    && !_connections.ContainsKey(fromEndPoint))
                {
                    TcpConnection newConnection = new TcpConnection(acceptedSocket, fromEndPoint, this);
                    _connections.Add(fromEndPoint, newConnection);
                    OnConnected(newConnection);
                }
                else
                    acceptedSocket.Close();
            }
        }

        /// <summary>Stops listening for connections.</summary>
        private void StopListening()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            Socket.Close();
        }

        /// <inheritdoc/>
        public void Close(Connection connection)
        {
            if (connection is TcpConnection tcpConnection)
            {
                _closedConnections.Add(tcpConnection.RemoteEndPoint);
                tcpConnection.Close();
            }
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            StopListening();
            _connections.Clear();
        }

        /// <summary>Invokes the <see cref="Connected"/> event.</summary>
        /// <param name="connection">The successfully established connection.</param>
        protected virtual void OnConnected(Connection connection)
        {
            Connected?.Invoke(this, new ConnectedEventArgs(connection));
        }

        /// <inheritdoc/>
        protected internal override void OnDataReceived(int amount, TcpConnection fromConnection)
        {
            if ((MessageHeader)(ReceiveBuffer[0] & Message.HeaderBitmask) == MessageHeader.Connect)
            {
                if (fromConnection.DidReceiveConnect)
                    return;

                fromConnection.DidReceiveConnect = true;
            }

            DataReceived?.Invoke(this, new DataReceivedEventArgs(ReceiveBuffer, amount, fromConnection));
        }
    }
}
