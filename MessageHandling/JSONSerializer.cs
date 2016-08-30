using System.Text;
using Newtonsoft.Json;

namespace ShopAtHome.MessageQueue.MessageHandling
{
    /// <summary>
    /// Small helper class for handling serialized data
    /// Specifically helpful for working with byte arrays
    /// Deals in JSON
    /// </summary>
    internal class JsonSerializer
    {
        private readonly Encoding _encoding;

        /// <summary>
        /// Initializes the serializer with a particular encoding
        /// </summary>
        /// <param name="encoding"></param>
        internal JsonSerializer(Encoding encoding)
        {
            _encoding = encoding;
        }

        internal Message<T> Get<T>(byte[] obj)
        {
            return Get<T>(_encoding.GetString(obj));
        }

        internal Message<T> Get<T>(string obj)
        {
            return JsonConvert.DeserializeObject<Message<T>>(obj);
        }

        internal byte[] Make<T>(Message<T> message)
        {
            var serialized = JsonConvert.SerializeObject(message);
            return _encoding.GetBytes(serialized);
        }
    }
}
