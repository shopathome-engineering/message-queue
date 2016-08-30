using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace ShopAtHome.MessageQueue.RabbitMQ
{
    /// <summary>
    /// We seem to be getting inexplicable "outage" periods where for a few milliseconds the RabbitMQ servers reports that it is unreachable.
    /// These helper methods are advisable to use whenever you communicate with the bus, as they will retry for up to 100ms if they encounter such a problem
    /// </summary>
    internal class RabbitMQHelper
    {
        private const int _NUM_MAX_RETRIES = 10;
        private const int _MILLISECONDS_RETRY_WAIT = 10;

        internal static T UseConnectionWithRetries<T>(Func<IModel, T> function, IModel connection)
        {
            var attempts = 0;
            while (attempts < _NUM_MAX_RETRIES)
            {
                try
                {
                    return function(connection);
                }
                catch (BrokerUnreachableException)
                {
                    if (attempts > _NUM_MAX_RETRIES)
                    {
                        throw;
                    }
                    attempts++;
                    Thread.Sleep(_MILLISECONDS_RETRY_WAIT);
                }
            }
            return function(connection);
        }

        internal static void UseConnectionWithRetries(Action<IModel> function, IModel connection)
        {
            var attempts = 0;
            while (attempts < _NUM_MAX_RETRIES)
            {
                try
                {
                    function(connection);
                    return;
                }
                catch (BrokerUnreachableException)
                {
                    if (attempts > _NUM_MAX_RETRIES)
                    {
                        throw;
                    }
                    attempts++;
                    Thread.Sleep(_MILLISECONDS_RETRY_WAIT);
                }
            }
            function(connection);
        }
    }
}
