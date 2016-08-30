namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// Information about binding a queue or message to an exchange
    /// </summary>
    public class ExchangeBindingInfo
    {
        /// <summary>
        /// The routing key used to either retrieve messages from an exchange (for queues)
        /// or to send messages to the correct queues (for messages)
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        /// The identifier (name) of the exchange to be bound to
        /// </summary>
        public string ExchangeIdentifier { get; set; }
    }
}
