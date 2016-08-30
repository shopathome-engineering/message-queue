namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// Contains meta information about the message, used to facilitate communication with the bus server
    /// </summary>
    public class MessageHeader
    {
        /// <summary>
        /// Routing information for this message. If left empty, default routing for the queue or exchange is used
        /// </summary>
        public string RoutingKey { get; set; }
    }
}
