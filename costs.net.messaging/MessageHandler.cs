namespace costs.net.messaging
{
    using System.Threading.Tasks;
    using core.Messaging;

    public abstract class MessageHandler<T> : IMessageHandler<T> where T : new()
    {
        public abstract Task Handle(T message);
    }
}
