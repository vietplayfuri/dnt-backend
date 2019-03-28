namespace costs.net.scheduler.core
{
    using Autofac;
    using AutoMapper;
    using Jobs;
    using net.core.Mapping;

    public class SchedulerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var configuration = new MapperConfiguration(config =>
                config.AddProfiles(
                    typeof(NotificationProfile),
                    typeof(RegionProfile)
                )
            );
            IMapper mapper = new Mapper(configuration);
            builder.RegisterInstance(mapper);

            builder.RegisterType<PurgeJob>().InstancePerDependency();
            builder.RegisterType<EmailNotificationReminderJob>().InstancePerDependency();
            builder.RegisterType<ActivityLogDeliveryJob>().InstancePerDependency();
        }
    }
}