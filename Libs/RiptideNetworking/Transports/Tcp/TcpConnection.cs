// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

using Riptide.Utils;
using System.Net;
using System.Net.Sockets;

namespace Riptide.Transports.Tcp
{
    /// <summary>Represents a connection to a <see cref="TcpServer"/> or <see cref="TcpClient"/>.</summary>
    public class TcpConnection : Connection, IEquatable<TcpConnection>
    {
        /// <summary>The endpoint representing the other end of the connection.</summary>
        public readonly IPEndPoint RemoteEndPoint;

        /// <summary>Whether or not the server has received a connection attempt from this connection.</summary>
        internal bool DidReceiveConnect;

        /// <summary>The socket to use for sending and receiving.</summary>
        private readonly Socket _socket;

        /// <summary>The local peer this connection is associated with.</summary>
        private readonly TcpPeer _peer;

        /// <summary>An array to receive message size values into.</summary>
        private readonly byte[] _sizeBytes = new byte[sizeof(int)];

        /// <summary>The size of the next message to be received.</summary>
        private int _nextMessageSize;

        /// <summary>Initializes the connection.</summary>
        /// <param name="socket">The socket to use for sending and receiving.</param>
        /// <param name="remoteEndPoint">The endpoint representing the other end of the connection.</param>
        /// <param name="peer">The local peer this connection is associated with.</param>
        internal TcpConnection(Socket socket, IPEndPoint remoteEndPoint, TcpPeer peer)
        {
            RemoteEndPoint = remoteEndPoint;
            this._socket = socket;
            this._peer = peer;
        }

        /// <inheritdoc/>
        protected internal override void Send(byte[] dataBuffer, int amount)
        {
            if (amount == 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Sending 0 bytes is not allowed!");

            try
            {
                if (_socket.Connected)
                {
                    Converter.FromInt(amount, _peer.SendBuffer, 0);
                    Array.Copy(dataBuffer, 0, _peer.SendBuffer, sizeof(int),
                        amount); // TODO: consider sending length separately with an extra socket.Send call instead of copying the data an extra time
                    _socket.Send(_peer.SendBuffer, amount + sizeof(int), SocketFlags.None);
                }
            }
            catch (SocketException)
            {
                // May want to consider triggering a disconnect here (perhaps depending on the type
                // of SocketException)? Timeout should catch disconnections, but disconnecting
                // explicitly might be better...
            }
        }

        /// <summary>Polls the socket and checks if any data was received.</summary>
        internal void Receive()
        {
            bool tryReceiveMore = true;
            while (tryReceiveMore)
            {
                int byteCount = 0;
                try
                {
                    if (_nextMessageSize > 0)
                    {
                        // We already have a size value
                        tryReceiveMore = TryReceiveMessage(out byteCount);
                    }
                    else if (_socket.Available >= sizeof(int))
                    {
                        // We have enough bytes for a complete size value
                        _socket.Receive(_sizeBytes, sizeof(int), SocketFlags.None);
                        _nextMessageSize = Converter.ToInt(_sizeBytes, 0);

                        if (_nextMessageSize > 0)
                            tryReceiveMore = TryReceiveMessage(out byteCount);
                    }
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
                            _peer.OnDisconnected(this, DisconnectReason.TransportError);
                            break;
                        case SocketError.ConnectionReset:
                            _peer.OnDisconnected(this, DisconnectReason.Disconnected);
                            break;
                        case SocketError.TimedOut:
                            _peer.OnDisconnected(this, DisconnectReason.TimedOut);
                            break;
                        case SocketError.MessageSize:
                        default:
                            break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    tryReceiveMore = false;
                    _peer.OnDisconnected(this, DisconnectReason.TransportError);
                }
                catch (NullReferenceException)
                {
                    tryReceiveMore = false;
                    _peer.OnDisconnected(this, DisconnectReason.TransportError);
                }

                if (byteCount > 0)
                    _peer.OnDataReceived(byteCount, this);
            }
        }

        /// <summary>Receives a message, if all of its data is ready to be received.</summary>
        /// <param name="receivedByteCount">How many bytes were received.</param>
        /// <returns>Whether or not all of the message's data was ready to be received.</returns>
        private bool TryReceiveMessage(out int receivedByteCount)
        {
            if (_socket.Available >= _nextMessageSize)
            {
                // We have enough bytes to read the complete message
                receivedByteCount = _socket.Receive(_peer.ReceiveBuffer, _nextMessageSize, SocketFlags.None);
                _nextMessageSize = 0;
                return true;
            }

            receivedByteCount = 0;
            return false;
        }

        /// <summary>Closes the connection.</summary>
        internal void Close()
        {
            _socket.Close();
        }

        /// <inheritdoc/>
        public override string ToString() => RemoteEndPoint.ToStringBasedOnIPFormat();

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as TcpConnection);

        /// <inheritdoc/>
        public bool Equals(TcpConnection? other)
        {
            if (other is null)
                return false;

            return ReferenceEquals(this, other) || RemoteEndPoint.Equals(other.RemoteEndPoint);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -288961498 + EqualityComparer<IPEndPoint>.Default.GetHashCode(RemoteEndPoint);
        }

        public static bool operator ==(TcpConnection? left, TcpConnection? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TcpConnection? left, TcpConnection? right)
        {
            return !Equals(left, right);
        }
    }
}
