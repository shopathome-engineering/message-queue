using System.Collections.Generic;
using System.Linq;

namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// A single message contained within a queue
    /// </summary>
    /// <typeparam name="TData">The type of data that the message contains</typeparam>
    public class Message<TData>
    {
        /// <summary>
        /// Mama says that null collections are the devil's right hand
        /// </summary>
        public Message()
        {
            Data = new List<TData>();
        } 

        /// <summary>
        /// Header info for the message
        /// </summary>
        public MessageHeader Header { get; set; }

        /// <summary>
        /// The data contained within the message
        /// </summary>
        public List<TData> Data { get; set; }

        /// <summary>
        /// The ID of the message as understood by the bus server. Assigned on read
        /// </summary>
        public ulong ConversationId { get; set; }

        /// <summary>
        /// Helper method to attach data to the message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Message<TData> WithData(TData item)
        {
            var result = new Message<TData>();
            result.Data.Add(item);
            return result;
        }

        /// <summary>
        /// Helper method to attach data to the message
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Message<TData> WithData(IEnumerable<TData> items)
        {
            var result = new Message<TData>();
            result.Data.AddRange(items);
            return result;
        }

        /// <summary>
        /// Helper method to set the routing key of the message
        /// </summary>
        /// <param name="routeKey"></param>
        /// <returns></returns>
        public Message<TData> WithRouteKey(string routeKey)
        {
            if (Header == null)
            {
                Header = new MessageHeader();
            }
            Header.RoutingKey = routeKey;
            return this;
        }

        /// <summary>
        /// The Message data type can contain an enumeration of its TData. However, we want our messages in the bus to have a size of a single TData, to ensure maximum concurrency and to avoid transactional issues.
        /// This method takes a Message with TData n > 1 and returns an enumeration of n Messages.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Message<TData>> DecomposeBatchIntoIndividualMessages()
        {
            return Data.Select(datum =>
            {
                var newMessage = WithData(datum);
                newMessage.Header = Header;
                return newMessage;
            });
        }
    }
}
