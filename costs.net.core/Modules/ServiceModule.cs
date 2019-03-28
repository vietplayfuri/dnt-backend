using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Options;
using Nest;
using Module = Autofac.Module;

namespace dnt.core.Modules
{
    public class ServiceModule : Module
    {
        // Add here any services that requires custom registration
        //private static readonly HashSet<Type> AutoRegistrationExclusion = new HashSet<Type>
        //{
        //    typeof(RuleService),
        //    typeof(CacheableRuleService)
        //};

        protected override void Load(ContainerBuilder builder)
        {
            var types = typeof(ServiceModule).GetTypeInfo().Assembly.GetTypes();

            // register services
            builder.RegisterTypes(types)
                .Where(t => t.Namespace != null && t.Namespace.Contains(nameof(Services)))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            //builder.RegisterType<GdamClient>().As<IGdamClient>().SingleInstance();

            //builder.Register(ctx => ElasticConnectionSettings.GetConnectionSettings(ctx.Resolve<IOptions<ElasticSearchSettings>>().Value)).As<IConnectionSettingsValues>().InstancePerLifetimeScope();
            //builder.RegisterType<ElasticClient>().As<IElasticClient>().InstancePerLifetimeScope();
            //builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerLifetimeScope();
            //builder.RegisterType<PaperpusherClient>().As<IPaperpusherClient>().InstancePerLifetimeScope();
            //builder.RegisterType<HttpContextApplicationUriHelper>().As<IApplicationUriHelper>().InstancePerLifetimeScope();
            //builder.RegisterType<DotLiquidMessageBuilder>().As<IActivityLogMessageBuilder>().InstancePerLifetimeScope();
            //builder.RegisterType<PartialMonthCalculator>().As<IMonthCalculator>().InstancePerLifetimeScope();

            //RegisterDecorator<IRuleService, RuleService, CacheableRuleService>(builder);
            //builder.RegisterType<CacheableRuleService>().As<ICacheable>().InstancePerLifetimeScope();
        }

        private static void RegisterDecorator<TInterface, TImplementation, TDecorator>(ContainerBuilder builder)
            where TImplementation : TInterface
            where TDecorator : TInterface
        {
            builder.RegisterType<TImplementation>()
                .Named<TInterface>("implementation")
                .InstancePerLifetimeScope();

            builder.RegisterType<TDecorator>()
                .Named<TInterface>("decorator")
                .InstancePerLifetimeScope();

            builder.RegisterDecorator<TInterface>(
                    (c, inner) => c.ResolveNamed<TInterface>("decorator", TypedParameter.From(inner)),
                    "implementation"
                )
                .InstancePerLifetimeScope();
        }
    }
}
