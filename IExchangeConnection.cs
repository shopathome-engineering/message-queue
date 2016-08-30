using System;

namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// Represents a write-only connection to an exchange (sits in front of queue(s) and directs messages appropriately, based on routing information)
    /// </summary>
    /// <typeparam name="TMessageData"></typeparam>
    public interface IExchangeConnection<TMessageData> : IDisposable
    {
        /// <summary>
        /// Writes the provided message to the exchange
        /// </summary>
        /// <param name="message"></param>
        void Write(Message<TMessageData> message);
    }
}
