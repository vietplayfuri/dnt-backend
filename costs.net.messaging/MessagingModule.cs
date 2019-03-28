namespace costs.net.messaging
{
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using core.Messaging;
    using Handlers;
    using Module = Autofac.Module;

    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            Configure(builder);
        }

        private static void Configure(ContainerBuilder container)
        {
            var handlers = typeof(MessagingModule).GetTypeInfo().Assembly
                .GetTypes()
                .Where(t => t.IsClosedTypeOf(typeof(IMessageHandler<>)))
                .ToArray();

            // Register handlers
            container.RegisterTypes(handlers)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            // Create safe handler for each handler
            foreach (var handler in handlers)
            {
                var paramTypes = handler
                    .GetInterfaces()
                    .First(t => t.IsClosedTypeOf(typeof(IMessageHandler<>)))
                    .GetGenericArguments();

                var safeHadnler = typeof(SafeHandler<>).MakeGenericType(paramTypes);
                var safeHandlerInterface = typeof(ISafeHandler<>).MakeGenericType(paramTypes);

                container
                    .RegisterType(safeHadnler)
                    .As(safeHandlerInterface)
                    .InstancePerLifetimeScope();
            }

            container.RegisterType<InternalMessageSender>().AsImplementedInterfaces().SingleInstance();

            container.RegisterType<InternalMessageReceiver>().AsImplementedInterfaces().SingleInstance();
            container.RegisterType<ExternalMessageReceiver>().AsImplementedInterfaces().SingleInstance();
            container.RegisterType<ExternalMessageSender>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
