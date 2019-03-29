namespace dnt.dataAccess
{
    using Autofac;
    using Module = Autofac.Module;

    public class DataAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            Configure(builder);
        }

        private static void Configure(ContainerBuilder container)
        {
            container.RegisterType<EFContext>()
                .As<EFContext>()
                .InstancePerLifetimeScope();
        }
    }

    /// <summary>
    /// Job need to be separated to avoid using the same efcontext and leading to unexpected error
    /// </summary>
    public class DataJobAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            Configure(builder);
        }

        private static void Configure(ContainerBuilder container)
        {
            container.RegisterType<EFContext>()
                .InstancePerDependency();
        }
    }

}
