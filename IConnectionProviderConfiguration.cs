namespace ShopAtHome.MessageQueue
{
    /// <summary>
    /// Implementations of this interface provide configuration information for implementations of the IConnectionProvider interface.
    /// </summary>
    public interface IConnectionProviderConfiguration
    {
        /// <summary>
        /// The address or identifier of the message bus used by the connection provider
        /// </summary>
        string Endpoint { get; }
        /// <summary>
        /// The username that should be used in authenticating with the message bus
        /// </summary>
        string Username { get; }
        /// <summary>
        /// The password that should be used in authenticating with the message bus
        /// </summary>
        string Password { get; }
    }
}
