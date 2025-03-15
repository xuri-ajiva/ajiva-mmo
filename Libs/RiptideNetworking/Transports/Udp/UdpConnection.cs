// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

using Riptide.Utils;
using System.Net;

namespace Riptide.Transports.Udp
{
    /// <summary>Represents a connection to a <see cref="UdpServer"/> or <see cref="UdpClient"/>.</summary>
    public class UdpConnection : Connection, IEquatable<UdpConnection>
    {
        /// <summary>The endpoint representing the other end of the connection.</summary>
        public readonly IPEndPoint RemoteEndPoint;

        /// <summary>The local peer this connection is associated with.</summary>
        private readonly UdpPeer _peer;

        /// <summary>Initializes the connection.</summary>
        /// <param name="remoteEndPoint">The endpoint representing the other end of the connection.</param>
        /// <param name="peer">The local peer this connection is associated with.</param>
        internal UdpConnection(IPEndPoint remoteEndPoint, UdpPeer peer)
        {
            RemoteEndPoint = remoteEndPoint;
            this._peer = peer;
        }

        /// <inheritdoc/>
        protected internal override void Send(byte[] dataBuffer, int amount)
        {
            _peer.Send(dataBuffer, amount, RemoteEndPoint);
        }

        /// <inheritdoc/>
        public override string ToString() => RemoteEndPoint.ToStringBasedOnIPFormat();

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as UdpConnection);
        /// <inheritdoc/>
        public bool Equals(UdpConnection? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return RemoteEndPoint.Equals(other.RemoteEndPoint);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -288961498 + EqualityComparer<IPEndPoint>.Default.GetHashCode(RemoteEndPoint);
        }

        public static bool operator ==(UdpConnection? left, UdpConnection? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UdpConnection? left, UdpConnection? right)
        {
            return !Equals(left, right);
        }
    }
}
