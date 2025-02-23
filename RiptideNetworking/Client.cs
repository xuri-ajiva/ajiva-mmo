// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

using Riptide.Transports;
using Riptide.Utils;
using Microsoft.Extensions.Logging;

namespace Riptide
{
    /// <summary>A client that can connect to a <see cref="Server"/>.</summary>
    public class Client : Peer
    {
        /// <summary>Invoked when a connection to the server is established.</summary>
        public event EventHandler? Connected;

        /// <summary>Invoked when a connection to the server fails to be established.</summary>
        public event EventHandler<ConnectionFailedEventArgs>? ConnectionFailed;

        /// <summary>Invoked when a message is received.</summary>
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        /// <summary>Invoked when disconnected from the server.</summary>
        public event EventHandler<DisconnectedEventArgs>? Disconnected;

        /// <summary>Invoked when another <i>non-local</i> client connects.</summary>
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

        /// <summary>Invoked when another <i>non-local</i> client disconnects.</summary>
        public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

        /// <summary>The client's numeric ID.</summary>
        public ushort Id => _connection!.Id;

        /// <inheritdoc cref="Riptide.Connection.Rtt"/>
        public short Rtt => _connection!.Rtt;

        /// <inheritdoc cref="Riptide.Connection.SmoothRtt"/>
        /// <remarks>This value is slower to accurately represent lasting changes in latency than <see cref="Rtt"/>, but it is less susceptible to changing drastically due to significant—but temporary—jumps in latency.</remarks>
        public short SmoothRtt => _connection!.SmoothRtt;

        /// <summary>Sets the client's <see cref="Connection.TimeoutTime"/>.</summary>
        public override int TimeoutTime
        {
            set
            {
                DefaultTimeout = value;
                _connection!.TimeoutTime = DefaultTimeout;
            }
        }

        /// <summary>Whether or not the client is currently <i>not</i> trying to connect, pending, nor actively connected.</summary>
        public bool IsNotConnected => _connection is null || _connection.IsNotConnected;

        /// <summary>Whether or not the client is currently in the process of connecting.</summary>
        public bool IsConnecting => !(_connection is null) && _connection.IsConnecting;

        /// <summary>Whether or not the client's connection is currently pending (waiting to be accepted/rejected by the server).</summary>
        public bool IsPending => !(_connection is null) && _connection.IsPending;

        /// <summary>Whether or not the client is currently connected.</summary>
        public bool IsConnected => !(_connection is null) && _connection.IsConnected;

        /// <summary>The client's connection to a server.</summary>
        // Not an auto property because properties can't be passed as ref/out parameters. Could
        // use a local variable in the Connect method, but that's arguably not any cleaner. This
        // property will also probably only be used rarely from outside the class/library.
        public Connection? Connection => _connection;

        /// <inheritdoc cref="Connection"/>
        private Connection? _connection;

        /// <summary>How many connection attempts have been made so far.</summary>
        private int _connectionAttempts;

        /// <summary>How many connection attempts to make before giving up.</summary>
        private int _maxConnectionAttempts;

        /// <summary>The underlying transport's client that is used for sending and receiving data.</summary>
        private IClient _transport;

        /// <summary>The message sent when connecting. May include custom data.</summary>
        private Message? _connectMessage;

        /// <summary>Handles initial setup.</summary>
        /// <param name="transport">The transport to use for sending and receiving data.</param>
        /// <param name="logger">A logger to be used</param>
        public Client(IClient transport, ILogger<Client> logger) : base(logger)
        {
            this._transport = transport;
        }

        /// <summary>Handles initial setup using the built-in UDP transport.</summary>
        /// <param name="logger">The logger</param>
        public Client(ILogger<Client> logger) : this(new Transports.Udp.UdpClient(), logger)
        {
        }

        /// <summary>Disconnects the client if it's connected and swaps out the transport it's using.</summary>
        /// <param name="newTransport">The new transport to use for sending and receiving data.</param>
        /// <remarks>This method does not automatically reconnect to the server. To continue communicating with the server, <see cref="Connect(string,int,Message)"/> must be called again.</remarks>
        public void ChangeTransport(IClient newTransport)
        {
            Disconnect();
            _transport = newTransport;
        }

        /// <summary>Attempts to connect to a server at the given host address.</summary>
        /// <param name="hostAddress">The host address to connect to.</param>
        /// <param name="maxConnectionAttempts">How many connection attempts to make before giving up.</param>
        /// <param name="message">Data that should be sent to the server with the connection attempt. Use <see cref="Message.Create()"/> to get an empty message instance.</param>
        /// <remarks>
        ///   <para>Riptide's default transport expects the host address to consist of an IP and port, separated by a colon. For example: <c>127.0.0.1:7777</c>. If you are using a different transport, check the relevant documentation for what information it requires in the host address.</para>
        /// </remarks>
        /// <returns><see langword="true"/> if a connection attempt will be made. <see langword="false"/> if an issue occurred (such as <paramref name="hostAddress"/> being in an invalid format) and a connection attempt will <i>not</i> be made.</returns>
        public bool Connect(string hostAddress, int maxConnectionAttempts = 5, Message? message = null)
        {
            Disconnect();

            SubToTransportEvents();

            if (!_transport.Connect(hostAddress, out _connection, out string connectError))
            {
                Logger.Log(LogLevel.Error, connectError);
                UnsubFromTransportEvents();
                return false;
            }

            this._maxConnectionAttempts = maxConnectionAttempts;
            _connectionAttempts = 0;
            _connection.Initialize(this, DefaultTimeout);
            IncreaseActiveCount();

            _connectMessage = Message.Create(MessageHeader.Connect);
            if (message is not null)
            {
                if (message.ReadBits != 0)
                    Logger.Log(LogLevel.Error,
                        $"Use the parameterless 'Message.Create()' overload when setting connection attempt data!");

                _connectMessage.AddMessage(message);
                message.Release();
            }

            StartTime();
            Heartbeat();
            Logger.Log(LogLevel.Information, $"Connecting to {_connection}...");
            return true;
        }

        /// <summary>Subscribes appropriate methods to the transport's events.</summary>
        private void SubToTransportEvents()
        {
            _transport.Connected += TransportConnected;
            _transport.ConnectionFailed += TransportConnectionFailed;
            _transport.DataReceived += HandleData;
            _transport.Disconnected += TransportDisconnected;
        }

        /// <summary>Unsubscribes methods from all of the transport's events.</summary>
        private void UnsubFromTransportEvents()
        {
            _transport.Connected -= TransportConnected;
            _transport.ConnectionFailed -= TransportConnectionFailed;
            _transport.DataReceived -= HandleData;
            _transport.Disconnected -= TransportDisconnected;
        }


        /// <inheritdoc/>
        internal override void Heartbeat()
        {
            if (IsConnecting)
            {
                // If still trying to connect, send connect messages instead of heartbeats
                if (_connectionAttempts < _maxConnectionAttempts)
                {
                    Send(_connectMessage!, false);
                    _connectionAttempts++;
                }
                else
                    LocalDisconnect(DisconnectReason.NeverConnected);
            }
            else if (IsPending)
            {
                // If waiting for the server to accept/reject the connection attempt
                if (_connection!.HasConnectAttemptTimedOut)
                {
                    LocalDisconnect(DisconnectReason.TimedOut);
                    return;
                }
            }
            else if (IsConnected)
            {
                // If connected and not timed out, send heartbeats
                if (_connection!.HasTimedOut)
                {
                    LocalDisconnect(DisconnectReason.TimedOut);
                    return;
                }

                _connection.SendHeartbeat();
            }

            ExecuteLater(HeartbeatInterval, new HeartbeatEvent(this));
        }

        /// <inheritdoc/>
        public override void Update()
        {
            base.Update();
            _transport.Poll();
            HandleMessages();
        }

        /// <inheritdoc/>
        protected override void Handle(Message message, MessageHeader header, Connection connection)
        {
            switch (header)
            {
                // User messages
                case MessageHeader.Unreliable:
                case MessageHeader.Reliable:
                    OnMessageReceived(message);
                    break;

                // Internal messages
                case MessageHeader.Ack:
                    connection.HandleAck(message);
                    break;
                case MessageHeader.Connect:
                    connection.SetPending();
                    break;
                case MessageHeader.Reject:
                    if (!IsConnected) // Don't disconnect if we are connected
                        LocalDisconnect(DisconnectReason.ConnectionRejected, message, (RejectReason)message.GetByte());
                    break;
                case MessageHeader.Heartbeat:
                    connection.HandleHeartbeatResponse(message);
                    break;
                case MessageHeader.Disconnect:
                    LocalDisconnect((DisconnectReason)message.GetByte(), message);
                    break;
                case MessageHeader.Welcome:
                    if (IsConnecting || IsPending)
                    {
                        connection.HandleWelcome(message);
                        OnConnected();
                    }

                    break;
                case MessageHeader.ClientConnected:
                    OnClientConnected(message.GetUShort());
                    break;
                case MessageHeader.ClientDisconnected:
                    OnClientDisconnected(message.GetUShort());
                    break;
                default:
                    Logger.Log(LogLevel.Warning,
                        $"Unexpected message header '{header}'! Discarding {message.BytesInUse} bytes.");
                    break;
            }

            message.Release();
        }

        /// <summary>Sends a message to the server.</summary>
        /// <inheritdoc cref="Connection.Send(Message, bool)"/>
        public ushort Send(Message message, bool shouldRelease = true) => _connection!.Send(message, shouldRelease);

        /// <summary>Disconnects from the server.</summary>
        public void Disconnect()
        {
            if (_connection == null || IsNotConnected)
                return;

            Send(Message.Create(MessageHeader.Disconnect));
            LocalDisconnect(DisconnectReason.Disconnected);
        }

        /// <inheritdoc/>
        internal override void Disconnect(Connection connection, DisconnectReason reason)
        {
            if (connection.IsConnected && connection.CanQualityDisconnect)
                LocalDisconnect(reason);
        }

        /// <summary>Cleans up the local side of the connection.</summary>
        /// <param name="reason">The reason why the client has disconnected.</param>
        /// <param name="message">The disconnection or rejection message, potentially containing extra data to be handled externally.</param>
        /// <param name="rejectReason">The reason why the connection was rejected (<i>if</i> it was rejected).</param>
        private void LocalDisconnect(DisconnectReason reason, Message? message = null,
            RejectReason rejectReason = RejectReason.NoConnection)
        {
            if (IsNotConnected)
                return;

            UnsubFromTransportEvents();
            DecreaseActiveCount();

            StopTime();
            _transport.Disconnect();

            _connection!.LocalDisconnect();

            if (reason == DisconnectReason.NeverConnected)
                OnConnectionFailed(RejectReason.NoConnection);
            else if (reason == DisconnectReason.ConnectionRejected)
                OnConnectionFailed(rejectReason, message);
            else
                OnDisconnected(reason, message);
        }

        /// <summary>What to do when the transport establishes a connection.</summary>
        private void TransportConnected(object? sender, EventArgs e)
        {
        }

        /// <summary>What to do when the transport fails to connect.</summary>
        private void TransportConnectionFailed(object? sender, EventArgs e)
        {
            LocalDisconnect(DisconnectReason.NeverConnected);
        }

        /// <summary>What to do when the transport disconnects.</summary>
        private void TransportDisconnected(object? sender, Transports.DisconnectedEventArgs e)
        {
            if (_connection == e.Connection)
                LocalDisconnect(e.Reason);
        }

        #region Events

        /// <summary>Invokes the <see cref="Connected"/> event.</summary>
        protected virtual void OnConnected()
        {
            _connectMessage?.Release();
            _connectMessage = null;
            Logger.Log(LogLevel.Information, "Connected successfully!");
            Connected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Invokes the <see cref="ConnectionFailed"/> event.</summary>
        /// <param name="reason">The reason for the connection failure.</param>
        /// <param name="message">Additional data related to the failed connection attempt.</param>
        protected virtual void OnConnectionFailed(RejectReason reason, Message? message = null)
        {
            _connectMessage?.Release();
            _connectMessage = null;
            Logger.Log(LogLevel.Information, $"Connection to server failed: {Helper.GetReasonString(reason)}.");
            ConnectionFailed?.Invoke(this, new ConnectionFailedEventArgs(reason, message));
        }

        /// <summary>Invokes the <see cref="MessageReceived"/> event and initiates handling of the received message.</summary>
        /// <param name="message">The received message.</param>
        protected virtual void OnMessageReceived(Message message)
        {
            ushort messageId = (ushort)message.GetVarULong();
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(_connection!, messageId, message));
        }

        /// <summary>Invokes the <see cref="Disconnected"/> event.</summary>
        /// <param name="reason">The reason for the disconnection.</param>
        /// <param name="message">Additional data related to the disconnection.</param>
        protected virtual void OnDisconnected(DisconnectReason reason, Message? message)
        {
            Logger.Log(LogLevel.Information, $"Disconnected from server: {Helper.GetReasonString(reason)}.");
            Disconnected?.Invoke(this, new DisconnectedEventArgs(reason, message));
        }

        /// <summary>Invokes the <see cref="ClientConnected"/> event.</summary>
        /// <param name="clientId">The numeric ID of the client that connected.</param>
        protected virtual void OnClientConnected(ushort clientId)
        {
            Logger.Log(LogLevel.Information, $"Client {clientId} connected.");
            ClientConnected?.Invoke(this, new ClientConnectedEventArgs(clientId));
        }

        /// <summary>Invokes the <see cref="ClientDisconnected"/> event.</summary>
        /// <param name="clientId">The numeric ID of the client that disconnected.</param>
        protected virtual void OnClientDisconnected(ushort clientId)
        {
            Logger.Log(LogLevel.Information, $"Client {clientId} disconnected.");
            ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(clientId));
        }

        #endregion
    }
}
