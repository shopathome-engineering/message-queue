using System;

namespace ShopAtHome.MessageQueue.Exceptions
{
    /// <summary>
    /// Indicates a failure in serializing or de-serializing a Message object
    /// </summary>
    public class MessageSerializationFailureException : Exception
    {
        /// <summary>
        /// Constructs a new MessageSerializationFailureException with the raw contents of the message that caused the failure
        /// </summary>
        /// <param name="rawBody"></param>
        /// <param name="innerException"></param>
        public MessageSerializationFailureException(byte[] rawBody, Exception innerException) : base(innerException.Message, innerException)
        {
            RawMessageBody = rawBody;
        }

        /// <summary>
        /// The raw binary contents of the message that caused the failure
        /// </summary>
        public byte[] RawMessageBody { get; }
    }
}
