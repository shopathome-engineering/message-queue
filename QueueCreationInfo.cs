namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// Information about creating a queue
    /// </summary>
    public class QueueCreationInfo
    {
        /// <summary>
        /// Initializes the creation info with all of its required data
        /// </summary>
        /// <param name="queueIdentifier"></param>
        /// <param name="durable"></param>
        public QueueCreationInfo(string queueIdentifier, bool durable)
        {
            Identifier = queueIdentifier;
            Durable = durable;
        }

        /// <summary>
        /// Initializes the creation info with all of its required data
        /// </summary>
        /// <param name="queueIdentifier"></param>
        /// <param name="durable"></param>
        /// <param name="exchangeInfo"></param>
        public QueueCreationInfo(string queueIdentifier, bool durable, ExchangeBindingInfo exchangeInfo) : this(queueIdentifier, durable)
        {
            BindingInfo = exchangeInfo;
        }

        /// <summary>
        /// If the queue should be made durable (persist through system loss). Incurs a performance penalty if set to true.
        /// </summary>
        public bool Durable { get; }

        /// <summary>
        /// The identifier (name) of the queue
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Binding information for attaching this queue to an exchange. If left NULL, the queue will not be bound, but can still be accessed directly by its identifier
        /// </summary>
        public ExchangeBindingInfo BindingInfo { get; set; }
    }
}
