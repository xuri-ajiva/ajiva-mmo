// This file is provided under The MIT License as part of RiptideNetworking.
// Copyright (c) Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub:
// https://github.com/RiptideNetworking/Riptide/blob/main/LICENSE.md

namespace Riptide.Utils
{
    /// <summary>Executes an action when invoked.</summary>
    internal abstract class DelayedEvent
    {
        /// <summary>Executes the action.</summary>
        public abstract void Invoke();
    }

    /// <summary>Resends a <see cref="PendingMessage"/> when invoked.</summary>
    internal class ResendEvent : DelayedEvent
    {
        /// <summary>The message to resend.</summary>
        private readonly PendingMessage _message;
        /// <summary>The time at which the resend event was queued.</summary>
        private readonly long _initiatedAtTime;

        /// <summary>Initializes the event.</summary>
        /// <param name="message">The message to resend.</param>
        /// <param name="initiatedAtTime">The time at which the resend event was queued.</param>
        public ResendEvent(PendingMessage message, long initiatedAtTime)
        {
            this._message = message;
            this._initiatedAtTime = initiatedAtTime;
        }

        /// <inheritdoc/>
        public override void Invoke()
        {
            if (_initiatedAtTime == _message.LastSendTime) // If this isn't the case then the message has been resent already
                _message.RetrySend();
        }
    }

    /// <summary>Executes a heartbeat when invoked.</summary>
    internal class HeartbeatEvent : DelayedEvent
    {
        /// <summary>The peer whose heart to beat.</summary>
        private readonly Peer _peer;

        /// <summary>Initializes the event.</summary>
        /// <param name="peer">The peer whose heart to beat.</param>
        public HeartbeatEvent(Peer peer)
        {
            this._peer = peer;
        }

        /// <inheritdoc/>
        public override void Invoke()
        {
            _peer.Heartbeat();
        }
    }
}
