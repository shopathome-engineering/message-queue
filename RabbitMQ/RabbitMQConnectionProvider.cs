using System.Collections.Generic;
using RabbitMQ.Client;

namespace ShopAtHome.MessageQueue.RabbitMQ
{
    /// <summary>
    /// An implementation of a connection factory for RabbitMQ
    /// </summary>
    public class RabbitMQConnectionProvider : IConnectionProvider
    {
        private readonly ConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes the factory against the specified RabbitMQ server
        /// </summary>
        /// <param name="endpoint">The server to which we will try to connect. Uses the default port.</param>
        /// <param name="username">The username that will be used in connecting to RabbitMQ</param>
        /// <param name="password">The password that will be used in connecting to RabbitMQ</param>
        public RabbitMQConnectionProvider(string endpoint, string username, string password)
        {
            _connectionFactory = new ConnectionFactory { HostName = endpoint, UserName = username, Password = password };
        }

        /// <summary>
        /// Initializes the connection provider with the specified configuration
        /// </summary>
        /// <param name="configuration"></param>
        public RabbitMQConnectionProvider(IConnectionProviderConfiguration configuration)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = configuration.Endpoint,
                UserName = configuration.Username,
                Password = configuration.Password
            };
        }

        /// <summary>
        /// Creates a connection of data type TDataType against the provided queue name
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public IQueueConnection<TDataType> ConnectToQueue<TDataType>(string queueName)
        {
            return new RabbitMQQueueConnection<TDataType>(_connectionFactory, queueName);
        }

        /// <summary>
        /// Returns a connection to the specified exchange that communicates in the specified message data type
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public IExchangeConnection<TDataType> ConnectToExchange<TDataType>(string exchangeName)
        {
            return new RabbitMQExchangeConnection<TDataType>(_connectionFactory, exchangeName);
        }

        /// <summary>
        /// Creates the specified queue on the RabbitMQ server if it does not exist. If binding information has been provided, this will attempt to bind the queue as directed
        /// </summary>
        /// <param name="queueInfo"></param>
        /// <remarks>If you try to create a queue that already exist but you specify different configuration values than what exists,
        /// this will throw</remarks>
        public void CreateQueue(QueueCreationInfo queueInfo)
        {
            using (var conn = _connectionFactory.CreateConnection())
            using (var channel = conn.CreateModel())
            {
                RabbitMQHelper.UseConnectionWithRetries(c => c.QueueDeclare(queueInfo.Identifier, queueInfo.Durable, false, false, null), channel);
                if (queueInfo.BindingInfo != null)
                {
                    RabbitMQHelper.UseConnectionWithRetries(c => c.QueueBind(queueInfo.Identifier, queueInfo.BindingInfo.ExchangeIdentifier, queueInfo.BindingInfo.RoutingKey), channel);
                }
            }
        }

        /// <summary>
        /// Tries to remove the specified queue from the bus server.
        /// </summary>
        /// <param name="queueIdentifier"></param>
        public void DeleteQueue(string queueIdentifier)
        {
            using (var conn = _connectionFactory.CreateConnection())
            using (var channel = conn.CreateModel())
            {
                RabbitMQHelper.UseConnectionWithRetries(c => c.QueueDelete(queueIdentifier), channel);
            }
        }

        /// <summary>
        /// Tries to create the exchange with the parameters specified on the bus server
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="alternateExchange"></param>
        /// <remarks>If you try to create a queue that already exist but you specify different configuration values than what exists,
        /// this will throw</remarks>
        public void CreateExchange(string name, string type, string alternateExchange = null)
        {
            using (var conn = _connectionFactory.CreateConnection())
            using (var channel = conn.CreateModel())
            {
                Dictionary<string, object> args = null ;
                if (!string.IsNullOrEmpty(alternateExchange))
                {
                    args = new Dictionary<string, object> {{"alternate-exchange", alternateExchange}};
                }
                RabbitMQHelper.UseConnectionWithRetries(c => c.ExchangeDeclare(name, type, true, false, args), channel);
            }
        }

        /// <summary>
        /// Rabbit MQ uses the "." character as its term separator in topic routing
        /// </summary>
        public string RoutingKeyTermSeparator => ".";

        /// <summary>
        /// Rabbit MQ uses the "*" character as its wildcard in topic routing and binding
        /// </summary>
        /// <remarks>For example, "logging.error.*" in a 3-term routing key will match all logging and all errors, with the wildcard being the 3rd term</remarks>
        public string RoutingKeyTermWildcard => "*";

        /// <summary>
        /// Rabbit MQ uses the "#" character as its "deliver me everything" wildcard in topic routing and binding
        /// </summary>
        /// <remarks>For example, "logging.#" in a 3-term routing key will match all logging, with the wildcard swallowing all other terms</remarks>
        public string RoutingKeyGlobalWildcard => "#";
    }
}
