namespace costs.net.plugins.Modules
{
    using System.Reflection;
    using Autofac;
    using core.Builders;
    using core.Messaging;
    using core.Models;
    using Module = Autofac.Module;

    public class PluginModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var types = GetType().GetTypeInfo().Assembly.GetTypes();

            // Register builders and services
            builder.RegisterTypes(types)
                .Where(t => t.Namespace != null && (t.Namespace.Contains(nameof(PG.Builders)) || t.Namespace.Contains(nameof(PG.Services))))
                .AsImplementedInterfaces()
                .WithMetadata<PluginMetadata>(c => c.For(p => p.BuType, BuType.Pg))
                .InstancePerLifetimeScope();

            // Register plugin event handlers
            builder.RegisterTypes(types)
                .Where(t => t.IsClosedTypeOf(typeof(IPluginMessageHandler<>)))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}