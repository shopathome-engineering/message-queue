# Summary
Provides interfaces and a RabbitMQ implementation for interaction with message queue systems. Provides methods for reading and writing messages and abstraction around the messages themselves.

This library is available on nuget.

----

# API Documentation
The entry point to the library is the IConnectionProvider interface. This interface contains methods which create read/write connections to the message queue, as well as methods that create or delete individual queues on the remote server. 

The implementation of this interface, RabbitMQConnectionProvider, requires three parameters: the server name of the RabbitMQ system, a username, and a password. There are two constructors which take these - one which has three string parameters, and one which takes an IConnectionProviderConfiguration (which is just an interface with those three parameters as properties).

Once you've constructed an IConnectionProvider, the next step is to create a connection so that you can read or write messages. Queue connections are strongly typed to the data that the messages within the queue contain, implement IDisposable, and identify a queue by name.

    var connectionProvider = new RabbitMQConnectionProvider("via12qarbt01.boulder.sah.com", "admin", "admin");
    using (var queueConnection = connectionProvider.ConnectToQueue<string>("DocumentationQueue"))
    {
        // [...]
    }

## Queue Data

RabbitMQ's messages are serialized in binary - this library abstracts that behind a strongly-typed wrapper class called Message<T>. Message contains a Header (which currently just has a RouteKey property - see the section on Exchanges for more detail), a List<T> Data, and a ConversationId that is RabbitMQ's internal message identifier. The Data property will contain the actual contents of the message. T is unconstrained. JSON is used as an intermediary serialization format, so any object that can be serialized by JSON.NET can be used as message data.

*NB: Although Data is a List<T>, in this implementation, the library enforces a policy of a single T per message. Messages which are written with N > 1 items in Data will be transformed into N messages internally.*

### Reading data from a queue
There are two methods for consuming data from a queue. The first is ReadNext(), and the second is Subscribe.

#### ReadNext()
This is the simpler of the two methods. ReadNext() returns the message at the head of the queue, or null if the queue is empty.

    var connectionProvider = new RabbitMQConnectionProvider("via12qarbt01.boulder.sah.com", "admin", "admin");
    using (var queueConnection = connectionProvider.ConnectToQueue<string>("DocumentationQueue"))
    {
        var message = queueConnection.ReadNext();
        if (message != null)
        {
            Console.WriteLine(string.Join(",", message.Data));
        }
    }

ReadNext() does not acknowledge (clear) messages after read - they are left in an unacknowledged state. For more information on how this aspect of RabbitMQ works, see the [official documentation](https://www.rabbitmq.com/confirms.html). You will need to call the ConfirmMessageReceipt() method on the received message to acknowledge it.

    using (var queueConnection = connectionProvider.ConnectToQueue<string>("DocumentationQueue"))
    {
        var message = queueConnection.ReadNext();
        // Perform whatever operations you want, if any
        queueConnection.ConfirmMessageReceipt(message);
    }

#### Subscribe()
This is a more complex way of consuming data from a queue. When you Subscribe() to a queue, the provided functions are executed on the head of the queue continuously. The message is then acknowledged automatically. Once the queue has been emptied, the system simply waits until a message is written and then immediately executes on it. This requires a persistent connection to the queue, but enables "active listening" without additional scaffolding such as polling. One example of accomplishing this is to use a reset event to block a thread inside an open queue connection.

    var resetEvent = new ManualResetEventSlim();
    using (var queueConnection = connectionProvider.ConnectToQueue<string>("DocumentationQueue"))
    {
        queueConnection.Subscribe(m => Console.WriteLine(string.Join(",", m.Data)), resetEvent.Set);
        resetEvent.Wait();
    }

The first parameter of Subscribe is the function that will be executed on each message. In this example we are simply writing out the message data to the console. The second parameter is called when the connection is closed, canceled, or interrupted. In this example, it simply fires the reset event so that the thread unblocks.
Which method of data consumption you choose in the creation of your application will depend on the application needs.

----

### Writing data
The Write message takes a Message object of the same <T> as the queue connection. There is a static method on Message which aids in constructing a Message<T> from T or an enumerable of T.

    using (var queueConnection = connectionProvider.ConnectToQueue<string>("DocumentationQueue"))
    {
        var messageData = "Hello world!";
        queueConnection.Write(Message<string>.WithData(messageData));
    }

#### Exchanges
A more advanced topic in RabbitMQ are "exchanges". Our library supports them as well. Information on how exchanges work can be found in the [official documentation](https://www.rabbitmq.com/tutorials/tutorial-four-dotnet.html). This library supports routing through exchanges via the message headers. There is a method on Message that assists in generating these route keys. Writing to exchanges requires the IExchangeConnection interface which the IConnectionProvider connects to in the same way as it connects to queues. This interface exposes only a Write() method identical to that on the IQueueConnection.

    using (var exchangeConnection = connectionProvider.ConnectToExchange<string>("DocumentationExchange"))
    {
        var messageData = "Hello world!";
        exchangeConnection .Write(Message<string>.WithData(messageData).WithRouteKey("my route key"));
    }

----

### Miscellaneous

Because this library uses JSON serialization as an intermediary serialization format for messages, it is required that the generic types on Connections are consistent between writing and reading - in other words, if I am writing Foo objects to the queue, I can only read Foo objects out of it. Queues should never contain mixed data types, as there is no way to filter messages within a queue (and attempting to do so is a recognized anti-pattern). In the event that a connection tries to read a message for which it is unable to deserialize the data, a MessageSerializationFailureException will be thrown. This exception will contain the raw binary serialization (before it is turned into JSON) of the message, which may aid in debugging a system that has violated this contract.