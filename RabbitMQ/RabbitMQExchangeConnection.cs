using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using ShopAtHome.MessageQueue.MessageHandling;

namespace ShopAtHome.MessageQueue.RabbitMQ
{
    /// <summary>
    /// Provides an implementation of the exchange connection interface for Rabbit MQ
    /// </summary>
    /// <typeparam name="TMessageData"></typeparam>
    public class RabbitMQExchangeConnection<TMessageData> : IExchangeConnection<TMessageData>
    {
        private readonly string _exchangeName;
        private readonly JsonSerializer _jsonSerializer;
        private readonly IModel _channel;
        private readonly object _syncLock = new object();
        private bool _disposing;
        private static readonly BasicProperties MessageWriteProperties = new BasicProperties { Persistent = true };

        internal RabbitMQExchangeConnection(IConnection mqConnection, string exchangeName)
        {
            _exchangeName = exchangeName;
            _jsonSerializer = new JsonSerializer(Encoding.UTF8);
            _channel = mqConnection.CreateModel();
        }

        /// <summary>
        /// Writes the provided message to the exchange
        /// </summary>
        /// <param name="message"></param>
        public void Write(Message<TMessageData> message)
        {
            var messages = message.DecomposeBatchIntoIndividualMessages();
            foreach (var messageBytes in messages.Select(m => _jsonSerializer.Make(m)))
            {
                var routingKey = message.Header?.RoutingKey ?? string.Empty;
                RabbitMQHelper.UseConnectionWithRetries(c => c.BasicPublish(_exchangeName, routingKey, MessageWriteProperties, messageBytes), _channel);
            }
        }

        /// <summary>
        /// Closes and disposes of its open connection to RabbitMQ
        /// </summary>
        public void Dispose()
        {
            lock (_syncLock)
            {
                if (_disposing)
                {
                    return;
                }

                _disposing = true;
                _channel.Dispose();
            }
        }
    }
}
