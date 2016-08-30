namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// A factory which can create connections to queues
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        /// Returns a connection to the specified queue that communicates in the specified message data type
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="queueName">The name of the queue on the bus server</param>
        /// <returns></returns>
        /// <remarks>Possible TODO: The data type is static for a queue, so it doesn't make sense to force the caller to set the data type *and* the queue.
        /// Some kind of map perhaps, but how does that strongly typed generic get passed back to the client automatically? Hmm</remarks>
        IQueueConnection<TDataType> ConnectToQueue<TDataType>(string queueName);

        /// <summary>
        /// Returns a connection to the specified exchange that communicates in the specified message data type
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        IExchangeConnection<TDataType> ConnectToExchange<TDataType>(string exchangeName);

        /// <summary>
        /// Tries to create the queue with the parameters specified on the bus server
        /// </summary>
        /// <param name="queueInfo"></param>
        void CreateQueue(QueueCreationInfo queueInfo);

        /// <summary>
        /// Tries to remove the specified queue from the bus server.
        /// </summary>
        /// <param name="queueIdentifier"></param>
        void DeleteQueue(string queueIdentifier);

        /// <summary>
        /// Tries to create the exchange with the parameters specified on the bus server
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="alternateExchange"></param>
        void CreateExchange(string name, string type, string alternateExchange = null); 

        /// <summary>
        /// The character used to separate routing keywords
        /// </summary>
        string RoutingKeyTermSeparator { get; }

        /// <summary>
        /// The character used as a wildcard in a routing key term
        /// </summary>
        string RoutingKeyTermWildcard { get; }

        /// <summary>
        /// The character used as a global wildcard in a routing key
        /// </summary>
        string RoutingKeyGlobalWildcard { get; }
    }
}
