using System;

namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// Represents an active connection to a message queue
    /// </summary>
    /// <typeparam name="TMessageData"></typeparam>
    public interface IQueueConnection<TMessageData> : IDisposable
    {
        /// <summary>
        /// Subscribes to the queue and will execute the provided action for each message it receives
        /// </summary>
        /// <param name="onReceive"></param>
        void Subscribe(Action<Message<TMessageData>> onReceive);

        /// <summary>
        /// Subscribes to the queue and will execute the provided action for each message it receives
        /// </summary>
        /// <param name="onReceive"></param>
        /// <param name="onSubscriptionCancel">Executed if the subscription is terminated</param>
        void Subscribe(Action<Message<TMessageData>> onReceive, Action onSubscriptionCancel);

        /// <summary>
        /// Returns the next message in the queue.
        /// </summary>
        /// <returns>Null if no message exists in the queue.</returns>
        Message<TMessageData> ReadNext();

        /// <summary>
        /// Writes the provided message to the queue
        /// </summary>
        /// <param name="message"></param>
        void Write(Message<TMessageData> message);

        /// <summary>
        /// Peeks at the next message in the queue and returns its header information
        /// </summary>
        /// <returns></returns>
        MessageHeader PeekNextHeader();

        /// <summary>
        /// Confirms the read of the specified message with the queue
        /// </summary>
        /// <param name="message"></param>
        void ConfirmMessageReceipt(Message<TMessageData> message);

        /// <summary>
        /// Returns information about the connected queue
        /// </summary>
        /// <returns></returns>
        QueueMetadata GetQueueInfo();

        /// <summary>
        /// Returns the specified message to the head of the queue
        /// </summary>
        /// <param name="message"></param>
        void ReturnMessageToQueue(Message<TMessageData> message);
    }
}
