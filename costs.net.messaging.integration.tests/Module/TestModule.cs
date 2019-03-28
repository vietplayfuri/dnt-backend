namespace costs.net.messaging.integration.tests.Module
{
    using System.Linq;
    using System.Reflection;

    using Autofac;
    using core.Messaging;
    using Handlers;
    using Stubs;
    using Module = Autofac.Module;

    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            Configure(builder);
        }

        private static void Configure(ContainerBuilder container)
        {
            var handlers = typeof(TestModule).GetTypeInfo().Assembly
                .GetTypes()
                .Where(t => t.IsClosedTypeOf(typeof(IMessageHandler<>)))
                .ToArray();

            // Register handlers
            container.RegisterTypes(handlers).AsImplementedInterfaces()
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

                container.RegisterType(safeHadnler)
                    .As(safeHandlerInterface)
                    .InstancePerLifetimeScope();
            }

            container.RegisterType<MessageSenderStub>().AsImplementedInterfaces().SingleInstance();
            container.RegisterType<MessageReceiverStub>().AsImplementedInterfaces().SingleInstance();
        }
    }
}
