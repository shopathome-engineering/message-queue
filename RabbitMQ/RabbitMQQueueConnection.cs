using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using ShopAtHome.MessageQueue.Exceptions;
using ShopAtHome.MessageQueue.MessageHandling;

namespace ShopAtHome.MessageQueue.RabbitMQ
{
    /// <summary>
    /// Provides an implementation of the queue connection interface for Rabbit MQ
    /// </summary>
    /// <typeparam name="TMessageData"></typeparam>
    public class RabbitMQQueueConnection<TMessageData> : IQueueConnection<TMessageData>
    {
        private readonly string _queueName;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private bool _disposing;
        private readonly object _syncLock = new object();
        private readonly JsonSerializer _jsonSerializer;
        private static readonly BasicProperties MessageWriteProperties = new BasicProperties {Persistent = true};

        /// <summary>
        /// Initializes the connection to the RabbitMQ queue specified by the queue name
        /// </summary>
        /// <param name="connectionFactory"></param>
        /// <param name="queueName"></param>
        public RabbitMQQueueConnection(IConnectionFactory connectionFactory, string queueName)
        {
            _queueName = queueName;
            _jsonSerializer = new JsonSerializer(Encoding.UTF8);
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
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
                _connection.Dispose();
            }
        }

        /// <summary>
        /// Subscribes to the queue and will execute the provided action for each message it receives
        /// </summary>
        /// <param name="onReceive"></param>
        public void Subscribe(Action<Message<TMessageData>> onReceive)
        {
            Subscribe(onReceive, null);
        }

        /// <summary>
        /// Subscribes to the queue and will execute the provided action for each message it receives
        /// </summary>
        /// <param name="onReceive"></param>
        /// <param name="onSubscriptionCancel">Executed if the subscription is terminated</param>
        public void Subscribe(Action<Message<TMessageData>> onReceive, Action onSubscriptionCancel)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (s, message) => onReceive(_jsonSerializer.Get<TMessageData>(message.Body));
            consumer.ConsumerCancelled += (sender, args) => onSubscriptionCancel();
            _channel.BasicConsume(_queueName, true, consumer);
        }

        /// <summary>
        /// Returns the next message in the queue.
        /// </summary>
        /// <returns>Null if no message exists in the queue.</returns>
        public Message<TMessageData> ReadNext()
        {
            var raw = RabbitMQHelper.UseConnectionWithRetries(c => c.BasicGet(_queueName, false), _channel);
            if (raw == null)
            {
                return null;
            }
            Message<TMessageData> message;
            try
            {
                message = _jsonSerializer.Get<TMessageData>(raw.Body);
            }
            catch (Exception ex)
            {
                RabbitMQHelper.UseConnectionWithRetries(c => c.BasicAck(raw.DeliveryTag, false), _channel);
                var msfe = new MessageSerializationFailureException(raw.Body, ex);
                throw msfe;
            }
            message.ConversationId = raw.DeliveryTag;
            return message;
        }

        /// <summary>
        /// Writes the provided message to the queue
        /// </summary>
        /// <param name="message"></param>
        public void Write(Message<TMessageData> message)
        {
            var messages = message.DecomposeBatchIntoIndividualMessages();
            foreach (var messageBytes in messages.Select(m => _jsonSerializer.Make(m)))
            {
                RabbitMQHelper.UseConnectionWithRetries(c => c.BasicPublish(string.Empty, _queueName, MessageWriteProperties, messageBytes), _channel);
            }
        }

        /// <summary>
        /// Peeks at the next message in the queue and returns its header information
        /// </summary>
        /// <returns></returns>
        public MessageHeader PeekNextHeader()
        {
            // TODO: This needs to read the next message in its queue, get its header data, then return the message to the queue as a Cancel so it gets put back at the head
            throw new NotImplementedException("TODO: This needs to read the next message in its queue, get its header data, then return the message to the queue as a Cancel so it gets put back at the head");
        }

        /// <summary>
        /// Confirms the read of the specified message with the queue
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>For RabbitMQ, this is required in order for the message to be cleared from the queue</remarks>
        public void ConfirmMessageReceipt(Message<TMessageData> message)
        {
            RabbitMQHelper.UseConnectionWithRetries(c => c.BasicAck(message.ConversationId, false), _channel);
        }

        /// <summary>
        /// Returns information about the connected queue
        /// </summary>
        /// <returns></returns>
        public QueueMetadata GetQueueInfo()
        {
            var count = RabbitMQHelper.UseConnectionWithRetries(c => c.MessageCount(_queueName), _channel);
            if (count > int.MaxValue)
            {
                // This scenario seems unlikely enough that I don't feel like coding around it. Are we ever going to have queues 
                // with more than two billion messages at a time? We can cross that bridge when we come to it
                throw new InvalidOperationException("Count of messages in the queue is greater than a 32bit signed integer.");
            }
            var result = new QueueMetadata((int)count, _queueName);
            return result;
        }
    }
}
