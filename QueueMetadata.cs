namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// Information about a queue
    /// </summary>
    public class QueueMetadata
    {
        /// <summary>
        /// Immutable object so values must be set in the ctor
        /// </summary>
        /// <param name="messageCount"></param>
        /// <param name="name"></param>
        public QueueMetadata(int messageCount, string name)
        {
            MessageCount = messageCount;
            Name = name;
        }

        /// <summary>
        /// How many messages are in the queue
        /// </summary>
        public int MessageCount { get; }

        /// <summary>
        /// The name of the queue
        /// </summary>
        public string Name { get; }
    }
}
