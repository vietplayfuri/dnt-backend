namespace dnt.api.Mapping
{
    using Autofac;
    using AutoMapper;
    using dnt.core.Mapping;

    public class MappingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            Configure(builder);
        }

        private static void Configure(ContainerBuilder builder)
        {
            var configuration = new MapperConfiguration(config =>
                config.AddProfiles(
                    typeof(UserProfile)
                )
            );

            IMapper mapper = new Mapper(configuration);
            builder.RegisterInstance(mapper);
        }
    }
}
